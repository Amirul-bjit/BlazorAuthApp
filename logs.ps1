param(
    [string]$Service = "blazorapp"
)

Write-Host "📋 Viewing logs for service: $Service" -ForegroundColor Green

# Check if the specified service container is running
$containerStatus = docker ps --filter "name=$Service" --format "{{.Names}}"

if (-not $containerStatus) {
    Write-Host "⚠️  Container '$Service' is not running. Available services:" -ForegroundColor Yellow
    
    # Show available services
    Write-Host "Available containers:" -ForegroundColor Gray
    docker-compose ps
    
    $choice = Read-Host "Do you want to start the containers? (y/N)"
    if ($choice -eq 'y' -or $choice -eq 'Y') {
        Write-Host "Starting containers..." -ForegroundColor Yellow
        docker-compose up -d
        Start-Sleep -Seconds 5
    } else {
        Write-Host "Exiting..." -ForegroundColor Gray
        exit 0
    }
}

# Show usage examples
Write-Host ""
Write-Host "Usage examples:" -ForegroundColor Cyan
Write-Host "  .\logs.ps1           # View Blazor app logs" -ForegroundColor Gray
Write-Host "  .\logs.ps1 pgdb      # View PostgreSQL logs" -ForegroundColor Gray
Write-Host "  .\logs.ps1 blazorapp # View Blazor app logs (explicit)" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to stop viewing logs" -ForegroundColor Yellow
Write-Host ""

# Tail logs for the specified service
docker-compose logs -f $Service
