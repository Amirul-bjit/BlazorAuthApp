Write-Host "🔍 Analyzing CSV files in old-bu folder..." -ForegroundColor Green

$oldBuPath = ".\old-bu"

if (-not (Test-Path $oldBuPath)) {
    Write-Host "❌ old-bu folder not found!" -ForegroundColor Red
    exit 1
}

# Get all CSV files in old-bu folder
$csvFiles = Get-ChildItem -Path $oldBuPath -Filter "*.csv"

if ($csvFiles.Count -eq 0) {
    Write-Host "❌ No CSV files found in old-bu folder!" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Found $($csvFiles.Count) CSV files:" -ForegroundColor Cyan

foreach ($file in $csvFiles) {
    Write-Host "  📄 $($file.Name)" -ForegroundColor White
    
    # Try to determine what type of data this is
    $sampleData = Get-Content $file.FullName -TotalCount 2
    if ($sampleData.Count -gt 1) {
        $headers = $sampleData[0] -split ","
        $recordCount = (Get-Content $file.FullName | Measure-Object -Line).Lines - 1
        
        Write-Host "    📊 Headers: $($headers[0..2] -join ', ')..." -ForegroundColor Gray
        Write-Host "    📈 Records: $recordCount" -ForegroundColor Gray
        
        # Suggest what table this might be
        if ($file.Name -match "AspNetUsers|user" -or $headers -contains '"Email"' -or $headers -contains '"UserName"') {
            Write-Host "    💡 Likely: AspNetUsers table" -ForegroundColor Yellow
        } elseif ($file.Name -match "Categories|category" -or $headers -contains '"Name"' -and $headers -contains '"Description"') {
            Write-Host "    💡 Likely: Categories table" -ForegroundColor Yellow
        } elseif ($file.Name -match "Blogs|blog" -or $headers -contains '"Title"' -or $headers -contains '"Content"') {
            Write-Host "    💡 Likely: Blogs table" -ForegroundColor Yellow
        } elseif ($file.Name -match "Comments|comment" -or $headers -contains '"BlogId"' -and $headers -contains '"Content"') {
            Write-Host "    💡 Likely: BlogComments table" -ForegroundColor Yellow
        } elseif ($file.Name -match "Likes|like" -or $headers -contains '"BlogId"' -and $headers -contains '"UserId"') {
            Write-Host "    💡 Likely: BlogLikes table" -ForegroundColor Yellow
        }
    }
    Write-Host ""
}

Write-Host "🎯 Migration Options:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Auto-detect and import all files:" -ForegroundColor White
Write-Host "   .\migrate-data.ps1 -All" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Import specific file as AspNetUsers:" -ForegroundColor White
Write-Host "   .\migrate-data.ps1 -CsvPath `".\old-bu\[filename].csv`" -Users" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Import with custom mapping (edit migrate-data.ps1):" -ForegroundColor White
Write-Host "   # Rename files to match expected names:" -ForegroundColor Gray
Write-Host "   # AspNetUsers.csv, Categories.csv, Blogs.csv, etc." -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Would you like to proceed with auto-import? (y/N)"
if ($choice -eq 'y' -or $choice -eq 'Y') {
    Write-Host "🚀 Starting auto-import..." -ForegroundColor Green
    & .\migrate-data.ps1 -All
} else {
    Write-Host "💡 Please review the files and choose your migration strategy." -ForegroundColor Cyan
}