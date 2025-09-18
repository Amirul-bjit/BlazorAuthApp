param(
    [string]$OutputDirectory = ".\old-bu",
    [string]$LocalConnectionString = "Server=(localdb)\mssqllocaldb;Database=BlazorAuthApp;Trusted_Connection=true;",
    [switch]$IncludeIdentity
)

Write-Host "📤 Exporting data from local SQL Server database..." -ForegroundColor Green

# Create output directory
if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    Write-Host "📁 Created directory: $OutputDirectory" -ForegroundColor Gray
}

# Tables to export (in dependency order)
$tables = @(
    @{
        Name = "AspNetRoles"
        Columns = @("Id", "Name", "NormalizedName", "ConcurrencyStamp")
        HasIdentity = $false
    },
    @{
        Name = "AspNetUsers"
        Columns = @("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
        HasIdentity = $false
    },
    @{
        Name = "AspNetUserRoles"
        Columns = @("UserId", "RoleId")
        HasIdentity = $false
    },
    @{
        Name = "Categories"
        Columns = @("Id", "Name", "Description", "IsDeleted", "CreatedAt", "UpdatedAt")
        HasIdentity = $true
    },
    @{
        Name = "Blogs"
        Columns = @("Id", "Title", "Content", "CategoryId", "AuthorId", "IsPublished", "IsDeleted", "CreatedAt", "UpdatedAt")
        HasIdentity = $true
    },
    @{
        Name = "BlogLikes"
        Columns = @("Id", "BlogId", "UserId", "CreatedAt")
        HasIdentity = $true
    },
    @{
        Name = "BlogComments"
        Columns = @("Id", "BlogId", "UserId", "Content", "ParentCommentId", "IsDeleted", "CreatedAt", "UpdatedAt")
        HasIdentity = $true
    }
)

# Function to export table to CSV
function Export-TableToCsv {
    param(
        [string]$tableName,
        [string[]]$columns,
        [string]$outputPath,
        [string]$connectionString
    )
    
    try {
        $columnList = $columns -join ", "
        $query = "SELECT $columnList FROM [$tableName]"
        
        Write-Host "  Exporting $tableName..." -ForegroundColor Gray
        
        # Use sqlcmd if available
        if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
            $tempFile = Join-Path $env:TEMP "export_$tableName.csv"
            
            # Export with headers
            sqlcmd -S "(localdb)\mssqllocaldb" -d "BlazorAuthApp" -Q "SET NOCOUNT ON; $query" -o $tempFile -s "," -W -h -1
            
            if (Test-Path $tempFile) {
                # Add headers
                $headers = '"' + ($columns -join '","') + '"'
                $content = Get-Content $tempFile
                $content = $headers, $content
                $content | Out-File -FilePath $outputPath -Encoding UTF8
                Remove-Item $tempFile -Force
                
                $recordCount = (Get-Content $outputPath | Measure-Object -Line).Lines - 1
                Write-Host "    ✅ Exported $recordCount records" -ForegroundColor Green
                return $true
            }
        } else {
            Write-Host "    ⚠️  sqlcmd not found. Using PowerShell method..." -ForegroundColor Yellow
            Export-TableWithPowerShell -tableName $tableName -columns $columns -outputPath $outputPath -connectionString $connectionString
        }
    } catch {
        Write-Host "    ❌ Error exporting $tableName`: $_" -ForegroundColor Red
        return $false
    }
}

# Function to export using PowerShell (fallback)
function Export-TableWithPowerShell {
    param(
        [string]$tableName,
        [string[]]$columns,
        [string]$outputPath,
        [string]$connectionString
    )
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $columnList = $columns -join ", "
        $query = "SELECT $columnList FROM [$tableName]"
        
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        
        $dataset.Tables[0] | Export-Csv -Path $outputPath -NoTypeInformation -Encoding UTF8
        
        $connection.Close()
        
        $recordCount = $dataset.Tables[0].Rows.Count
        Write-Host "    ✅ Exported $recordCount records" -ForegroundColor Green
        return $true
        
    } catch {
        Write-Host "    ❌ Error with PowerShell export: $_" -ForegroundColor Red
        return $false
    }
}

# Function to generate SQL INSERT statements
function Export-TableToSql {
    param(
        [string]$tableName,
        [string[]]$columns,
        [string]$outputPath,
        [string]$connectionString,
        [bool]$hasIdentity
    )
    
    try {
        Write-Host "  Generating SQL for $tableName..." -ForegroundColor Gray
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $columnList = $columns -join ", "
        $query = "SELECT $columnList FROM [$tableName]"
        
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $reader = $command.ExecuteReader()
        
        $sqlStatements = @()
        
        if ($hasIdentity -and $IncludeIdentity) {
            $sqlStatements += "SET IDENTITY_INSERT `"$tableName`" ON;"
        }
        
        $recordCount = 0
        while ($reader.Read()) {
            $values = @()
            foreach ($column in $columns) {
                $value = $reader[$column]
                if ($value -eq [DBNull]::Value -or $value -eq $null) {
                    $values += "NULL"
                } elseif ($value -is [string]) {
                    $escapedValue = $value -replace "'", "''"
                    $values += "'$escapedValue'"
                } elseif ($value -is [bool]) {
                    $values += if ($value) { "true" } else { "false" }
                } elseif ($value -is [DateTime]) {
                    $values += "'$($value.ToString("yyyy-MM-dd HH:mm:ss"))'"
                } else {
                    $values += "'$value'"
                }
            }
            
            $columnNames = ($columns | ForEach-Object { "`"$_`"" }) -join ", "
            $valueList = $values -join ", "
            $sqlStatements += "INSERT INTO `"$tableName`" ($columnNames) VALUES ($valueList) ON CONFLICT DO NOTHING;"
            $recordCount++
        }
        
        if ($hasIdentity -and $IncludeIdentity) {
            $sqlStatements += "SET IDENTITY_INSERT `"$tableName`" OFF;"
        }
        
        $reader.Close()
        $connection.Close()
        
        $sqlStatements | Out-File -FilePath $outputPath -Encoding UTF8
        Write-Host "    ✅ Generated $recordCount INSERT statements" -ForegroundColor Green
        return $true
        
    } catch {
        Write-Host "    ❌ Error generating SQL for $tableName`: $_" -ForegroundColor Red
        return $false
    }
}

# Check if local database exists
try {
    $testConnection = New-Object System.Data.SqlClient.SqlConnection($LocalConnectionString)
    $testConnection.Open()
    $testConnection.Close()
    Write-Host "✅ Connected to local database successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Cannot connect to local database: $_" -ForegroundColor Red
    Write-Host "💡 Please update the connection string in the script" -ForegroundColor Yellow
    Write-Host "💡 Current connection string: $LocalConnectionString" -ForegroundColor Gray
    exit 1
}

# Export each table
$successCount = 0
$totalTables = $tables.Count

foreach ($table in $tables) {
    Write-Host "📊 Processing table: $($table.Name)" -ForegroundColor Cyan
    
    # Export to CSV
    $csvPath = Join-Path $OutputDirectory "$($table.Name).csv"
    $csvSuccess = Export-TableToCsv -tableName $table.Name -columns $table.Columns -outputPath $csvPath -connectionString $LocalConnectionString
    
    # Export to SQL
    $sqlPath = Join-Path $OutputDirectory "$($table.Name).sql"
    $sqlSuccess = Export-TableToSql -tableName $table.Name -columns $table.Columns -outputPath $sqlPath -connectionString $LocalConnectionString -hasIdentity $table.HasIdentity
    
    if ($csvSuccess -or $sqlSuccess) {
        $successCount++
    }
}

Write-Host ""
Write-Host "📋 Export Summary:" -ForegroundColor Cyan
Write-Host "  Total tables: $totalTables" -ForegroundColor Gray
Write-Host "  Successfully exported: $successCount" -ForegroundColor Green
Write-Host "  Output directory: $OutputDirectory" -ForegroundColor Gray

if ($successCount -gt 0) {
    Write-Host ""
    Write-Host "✅ Export completed!" -ForegroundColor Green
    Write-Host "💡 Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Review exported files in $OutputDirectory" -ForegroundColor Gray
    Write-Host "  2. Run: .\migrate-data.ps1 -All" -ForegroundColor Gray
    Write-Host "  3. Verify data in pgAdmin at http://localhost:8080" -ForegroundColor Gray
} else {
    Write-Host "❌ Export failed. Please check the errors above." -ForegroundColor Red
}