Write-Host "🔧 Preparing CSV files for PostgreSQL import..." -ForegroundColor Green

$oldBuPath = ".\old-bu"

# Function to normalize boolean values in CSV
function Fix-BooleanValues {
    param([string]$filePath)
    
    if (-not (Test-Path $filePath)) {
        return
    }
    
    Write-Host "📝 Fixing boolean values in $filePath..." -ForegroundColor Yellow
    
    $content = Get-Content $filePath -Raw
    
    # Replace True/False with true/false
    $content = $content -replace '\bTrue\b', 'true'
    $content = $content -replace '\bFalse\b', 'false'
    
    # Write back to file
    $content | Set-Content $filePath -Encoding UTF8
    
    Write-Host "✅ Fixed boolean values in $([System.IO.Path]::GetFileName($filePath))" -ForegroundColor Green
}

# Function to create schema-compatible CSV
function Create-SchemaCompatibleCsv {
    param(
        [string]$sourceFile,
        [string]$targetFile,
        [hashtable]$columnMapping
    )
    
    if (-not (Test-Path $sourceFile)) {
        Write-Host "⚠️  Source file not found: $sourceFile" -ForegroundColor Yellow
        return
    }
    
    Write-Host "🔄 Creating schema-compatible version: $targetFile" -ForegroundColor Cyan
    
    $csvData = Import-Csv $sourceFile
    $outputData = @()
    
    foreach ($row in $csvData) {
        $newRow = @{}
        foreach ($targetColumn in $columnMapping.Keys) {
            $sourceColumn = $columnMapping[$targetColumn]
            if ($sourceColumn -and $row.$sourceColumn) {
                $value = $row.$sourceColumn
                # Fix boolean values
                if ($value -eq "True") { $value = "true" }
                if ($value -eq "False") { $value = "false" }
                $newRow[$targetColumn] = $value
            } else {
                $newRow[$targetColumn] = $null
            }
        }
        $outputData += New-Object PSObject -Property $newRow
    }
    
    $outputData | Export-Csv $targetFile -NoTypeInformation -Encoding UTF8
    Write-Host "✅ Created $targetFile with $($outputData.Count) records" -ForegroundColor Green
}

# Process Categories.csv
$categoriesCsv = Join-Path $oldBuPath "Categories.csv"
if (Test-Path $categoriesCsv) {
    # Fix boolean values in original file
    Fix-BooleanValues $categoriesCsv
    
    # Create schema-compatible version
    $categoriesMapping = @{
        "Id" = "Id"
        "Name" = "Name" 
        "Description" = "Description"
        "IsDeleted" = "IsDeleted"
        "CreatedAt" = "CreatedAt"
        "UpdatedAt" = "UpdatedAt"
    }
    
    $compatibleCategoriesCsv = Join-Path $oldBuPath "Categories_Compatible.csv"
    Create-SchemaCompatibleCsv -sourceFile $categoriesCsv -targetFile $compatibleCategoriesCsv -columnMapping $categoriesMapping
}

# Process other CSV files and fix boolean values
$csvFiles = Get-ChildItem -Path $oldBuPath -Filter "*.csv"
foreach ($file in $csvFiles) {
    if ($file.Name -ne "Categories_Compatible.csv") {
        Fix-BooleanValues $file.FullName
    }
}

Write-Host ""
Write-Host "✅ CSV preparation completed!" -ForegroundColor Green
Write-Host "💡 Files are now ready for PostgreSQL import" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Next step: Run the migration" -ForegroundColor Yellow
Write-Host "  .\migrate-data.ps1 -All" -ForegroundColor Gray