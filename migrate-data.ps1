param(
    [string]$CsvPath = "",
    [switch]$Users,
    [switch]$Categories,
    [switch]$Blogs,
    [switch]$All,
    [string]$ImportDirectory = ".\old-bu"
)

Write-Host "🔄 Starting data migration to containerized PostgreSQL..." -ForegroundColor Green

# Check if PostgreSQL container is running
$pgContainer = docker ps --filter "name=blazorauthapp-pgdb-1" --format "{{.Names}}"
if (-not $pgContainer) {
    Write-Host "❌ PostgreSQL container is not running. Please start it first with .\run.ps1" -ForegroundColor Red
    exit 1
}

# Function to import CSV data to PostgreSQL
function Import-CsvToPostgres {
    param(
        [string]$csvFile,
        [string]$tableName,
        [string[]]$columns
    )
    
    if (-not (Test-Path $csvFile)) {
        Write-Host "❌ CSV file not found: $csvFile" -ForegroundColor Red
        return $false
    }
    
    Write-Host "📊 Importing $csvFile to table $tableName..." -ForegroundColor Yellow
    
    try {
        # Read CSV and convert to SQL INSERT statements
        $csvData = Import-Csv $csvFile
        $insertStatements = @()
        
        if ($csvData.Count -eq 0) {
            Write-Host "⚠️  No data found in CSV file" -ForegroundColor Yellow
            return $true
        }
        
        foreach ($row in $csvData) {
            $values = @()
            foreach ($column in $columns) {
                $value = $row.$column
                if ($value -eq $null -or $value -eq "NULL" -or $value -eq "") {
                    $values += "NULL"
                } elseif ($value -eq "True" -or $value -eq "true") {
                    $values += "true"
                } elseif ($value -eq "False" -or $value -eq "false") {
                    $values += "false"
                } else {
                    # Escape single quotes and wrap in quotes
                    $escapedValue = $value -replace "'", "''"
                    $values += "'$escapedValue'"
                }
            }
            
            $columnList = ($columns | ForEach-Object { "`"$_`"" }) -join ", "
            $valueList = $values -join ", "
            $insertStatements += "INSERT INTO `"$tableName`" ($columnList) VALUES ($valueList) ON CONFLICT DO NOTHING;"
        }
        
        # Create SQL file
        $sqlFile = Join-Path $env:TEMP "import_$tableName.sql"
        $insertStatements | Out-File -FilePath $sqlFile -Encoding UTF8
        
        # Execute SQL in PostgreSQL container using PowerShell-compatible method
        try {
            # Copy SQL file to container and execute it
            docker cp $sqlFile blazorauthapp-pgdb-1:/tmp/import.sql
            $result = docker exec blazorauthapp-pgdb-1 psql -U user -d database -f /tmp/import.sql
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Successfully imported $($csvData.Count) records to $tableName" -ForegroundColor Green
                Remove-Item $sqlFile -Force -ErrorAction SilentlyContinue
                # Clean up temp file in container
                docker exec blazorauthapp-pgdb-1 rm -f /tmp/import.sql
                return $true
            } else {
                Write-Host "❌ Failed to import data to $tableName" -ForegroundColor Red
                Write-Host "SQL file saved at: $sqlFile for debugging" -ForegroundColor Gray
                return $false
            }
        } catch {
            Write-Host "❌ Error executing SQL: $_" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error importing to $tableName`: $_" -ForegroundColor Red
        return $false
    }
}

# Function to import specific table
function Import-Table {
    param(
        [string]$tableName,
        [string[]]$columns,
        [string]$directory
    )
    
    $csvFile = Join-Path $directory "$tableName.csv"
    return Import-CsvToPostgres -csvFile $csvFile -tableName $tableName -columns $columns
}

# Main migration logic
if ($CsvPath -ne "" -and $Users) {
    # Import specific CSV file as Users table
    $userColumns = @("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
    Import-CsvToPostgres -csvFile $CsvPath -tableName "AspNetUsers" -columns $userColumns
} elseif ($All -or (-not $Users -and -not $Categories -and -not $Blogs)) {
    Write-Host "🔍 Looking for CSV files in directory: $ImportDirectory" -ForegroundColor Yellow
    
    # Check what CSV files exist and try to match them
    $foundFiles = Get-ChildItem -Path $ImportDirectory -Filter "*.csv" -ErrorAction SilentlyContinue
    
    if ($foundFiles.Count -eq 0) {
        Write-Host "❌ No CSV files found in $ImportDirectory" -ForegroundColor Red
        Write-Host "💡 Available files:" -ForegroundColor Yellow
        Get-ChildItem -Path $ImportDirectory -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  📄 $($_.Name)" -ForegroundColor Gray }
        exit 1
    }
    
    Write-Host "📁 Found $($foundFiles.Count) CSV files:" -ForegroundColor Green
    foreach ($file in $foundFiles) {
        Write-Host "  📄 $($file.Name)" -ForegroundColor White
    }
    Write-Host ""
    
    # Import tables in dependency order - try to find matching files
    $importTables = @(
        @{
            Name = "AspNetRoles"
            Columns = @("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            PossibleNames = @("AspNetRoles.csv", "Roles.csv", "roles.csv")
        },
        @{
            Name = "AspNetUsers"
            Columns = @("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
            PossibleNames = @("AspNetUsers.csv", "Users.csv", "users.csv", "data-*.csv")
        },
        @{
            Name = "AspNetUserRoles"
            Columns = @("UserId", "RoleId")
            PossibleNames = @("AspNetUserRoles.csv", "UserRoles.csv", "userroles.csv")
        },
        @{
            Name = "Categories"
            Columns = @("Id", "Name", "Description", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "DeletedAt", "DeletedBy", "IsDeleted")
            PossibleNames = @("Categories.csv", "categories.csv", "Category.csv")
        },
        @{
            Name = "Blogs"
            Columns = @("Id", "Title", "Content", "CategoryId", "AuthorId", "IsPublished", "IsDeleted", "CreatedAt", "UpdatedAt")
            PossibleNames = @("Blogs.csv", "blogs.csv", "Blog.csv", "Posts.csv")
        },
        @{
            Name = "BlogLikes"
            Columns = @("Id", "BlogId", "UserId", "CreatedAt")
            PossibleNames = @("BlogLikes.csv", "bloglikes.csv", "Likes.csv", "likes.csv")
        },
        @{
            Name = "BlogComments"
            Columns = @("Id", "BlogId", "UserId", "Content", "ParentCommentId", "IsDeleted", "CreatedAt", "UpdatedAt")
            PossibleNames = @("BlogComments.csv", "blogcomments.csv", "Comments.csv", "comments.csv")
        }
    )
    
    $importedCount = 0
    $totalTables = $importTables.Count
    
    foreach ($table in $importTables) {
        $foundFile = $null
        
        # Try to find a matching file
        foreach ($possibleName in $table.PossibleNames) {
            if ($possibleName -like "*-*") {
                # Handle wildcard patterns like data-*.csv
                $matchingFiles = Get-ChildItem -Path $ImportDirectory -Filter $possibleName -ErrorAction SilentlyContinue
                if ($matchingFiles.Count -gt 0) {
                    $foundFile = $matchingFiles[0].FullName
                    break
                }
            } else {
                $testPath = Join-Path $ImportDirectory $possibleName
                if (Test-Path $testPath) {
                    $foundFile = $testPath
                    break
                }
            }
        }
        
        if ($foundFile) {
            Write-Host "📁 Found: $foundFile → $($table.Name)" -ForegroundColor Green
            
            # Check if the CSV has the expected columns
            $csvHeaders = (Get-Content $foundFile -TotalCount 1) -replace '"', '' -split ','
            $missingColumns = $table.Columns | Where-Object { $_ -notin $csvHeaders }
            
            if ($missingColumns.Count -gt 0) {
                Write-Host "⚠️  Warning: Missing columns in $($table.Name): $($missingColumns -join ', ')" -ForegroundColor Yellow
                Write-Host "   Available columns: $($csvHeaders -join ', ')" -ForegroundColor Gray
                
                # Try to import with available columns only
                $availableColumns = $table.Columns | Where-Object { $_ -in $csvHeaders }
                if ($availableColumns.Count -gt 0) {
                    Write-Host "   Importing with available columns: $($availableColumns -join ', ')" -ForegroundColor Cyan
                    if (Import-CsvToPostgres -csvFile $foundFile -tableName $table.Name -columns $availableColumns) {
                        $importedCount++
                    }
                } else {
                    Write-Host "   ❌ No matching columns found, skipping $($table.Name)" -ForegroundColor Red
                }
            } else {
                # All columns available, proceed normally
                if (Import-CsvToPostgres -csvFile $foundFile -tableName $table.Name -columns $table.Columns) {
                    $importedCount++
                }
            }
        } else {
            Write-Host "⚠️  CSV file not found for table: $($table.Name)" -ForegroundColor Yellow
            Write-Host "   Looking for: $($table.PossibleNames -join ', ')" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "📋 Import Summary:" -ForegroundColor Cyan
    Write-Host "  Total tables checked: $totalTables" -ForegroundColor Gray
    Write-Host "  Successfully imported: $importedCount" -ForegroundColor Green
    
} elseif ($Users) {
    # Import only Users
    $userColumns = @("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
    Import-Table -tableName "AspNetUsers" -columns $userColumns -directory $ImportDirectory
    
} elseif ($Categories) {
    # Import only Categories
    $categoryColumns = @("Id", "Name", "Description", "IsDeleted", "CreatedAt", "UpdatedAt")
    Import-Table -tableName "Categories" -columns $categoryColumns -directory $ImportDirectory
    
} elseif ($Blogs) {
    # Import only Blogs
    $blogColumns = @(
        "Id", "Title", "Content", "Summary", "FeaturedImageUrl", "AuthorId", "IsPublished", "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted", "DeletedAt", "DeletedBy", "MetaDescription", "Slug", "ViewCount", "LikeCount", "EstimatedReadTime"
    )
    Import-Table -tableName "Blogs" -columns $blogColumns -directory $ImportDirectory
}

Write-Host ""
Write-Host "✅ Data migration completed!" -ForegroundColor Green
Write-Host "💡 You can verify the data in pgAdmin at http://localhost:8080" -ForegroundColor Cyan
Write-Host "💡 Or check the data with: docker exec -it blazorauthapp-pgdb-1 psql -U user -d database" -ForegroundColor Cyan