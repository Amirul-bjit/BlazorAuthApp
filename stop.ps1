Write-Host "🛑 Stopping all containers..." -ForegroundColor Yellow

# Stop and remove all containers
docker-compose down

# Optional: Remove volumes (uncomment if you want to reset the database)
# docker-compose down -v

Write-Host "✅ All containers stopped and removed." -ForegroundColor Green
Write-Host "💡 Database data is preserved in Docker volume 'blazorauthapp_pgdata'" -ForegroundColor Gray
Write-Host "💡 To completely reset the database, run: docker-compose down -v" -ForegroundColor Gray
