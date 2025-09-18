Write-Host "🧹 Cleaning up existing containers and networks..." -ForegroundColor Yellow

# Stop and remove all containers
Write-Host "Stopping containers..." -ForegroundColor Gray
docker-compose down 2>$null

# Remove any orphaned containers
Write-Host "Removing orphaned containers..." -ForegroundColor Gray
docker container prune -f

# Remove conflicting networks
Write-Host "Cleaning up networks..." -ForegroundColor Gray
docker network ls --filter "name=blazorauthapp" --format "{{.Name}}" | ForEach-Object {
    if ($_ -ne "") {
        Write-Host "Removing network: $_" -ForegroundColor Gray
        docker network rm $_ 2>$null
    }
}

# Optional: Remove volumes (this will delete your database data)
$cleanVolumes = Read-Host "Do you want to clean volumes (this will delete database data)? (y/N)"
if ($cleanVolumes -eq 'y' -or $cleanVolumes -eq 'Y') {
    Write-Host "Removing volumes..." -ForegroundColor Gray
    docker volume prune -f
    Write-Host "✅ Volumes cleaned" -ForegroundColor Green
} else {
    Write-Host "📦 Volumes preserved" -ForegroundColor Cyan
}

# Clean up any dangling images
Write-Host "Cleaning up dangling images..." -ForegroundColor Gray
docker image prune -f

Write-Host "✅ Cleanup completed!" -ForegroundColor Green
Write-Host "💡 You can now run .\run.ps1 to start fresh containers" -ForegroundColor Cyan