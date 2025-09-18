# Check if Docker is running
try {
    docker version > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not running"
    }
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

Write-Host "🚀 Starting PostgreSQL database, pgAdmin, and Blazor application..." -ForegroundColor Green

# Stop any existing containers to avoid conflicts
Write-Host "🛑 Stopping any existing containers..." -ForegroundColor Yellow
docker-compose down 2>$null

# Build and run all containers in the background
Write-Host "🔧 Building and starting containers..." -ForegroundColor Yellow
docker-compose up --build -d

# Check if containers are running
Write-Host "📋 Checking container status..." -ForegroundColor Yellow
docker-compose ps

# Wait for PostgreSQL to be ready
Write-Host "⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
$retries = 30
$ready = $false

for ($i = 1; $i -le $retries; $i++) {
    # Check if PostgreSQL container is running first
    $pgContainer = docker ps --filter "name=blazorauthapp-pgdb-1" --format "{{.Names}}"
    
    if (-not $pgContainer) {
        Write-Host "❌ PostgreSQL container is not running. Checking logs..." -ForegroundColor Red
        docker-compose logs pgdb
        Write-Host "Attempting to restart PostgreSQL..." -ForegroundColor Yellow
        docker-compose up -d pgdb
        Start-Sleep -Seconds 3
        continue
    }
    
    try {
        $result = docker exec blazorauthapp-pgdb-1 pg_isready -U user -d database 2>$null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }
    } catch {
        # Continue waiting
    }
    
    Write-Host "Attempt $i/$retries - PostgreSQL not ready yet..." -ForegroundColor Yellow
    
    # Show logs every 5 attempts for debugging
    if ($i % 5 -eq 0) {
        Write-Host "📋 PostgreSQL logs (last 10 lines):" -ForegroundColor Gray
        docker-compose logs --tail=10 pgdb
    }
    
    Start-Sleep -Seconds 2
}

if ($ready) {
    Write-Host "✅ PostgreSQL is ready!" -ForegroundColor Green
    
    # Wait a bit for pgAdmin to start
    Write-Host "⏳ Waiting for pgAdmin to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Run database migrations
    Write-Host "🔄 Applying database migrations..." -ForegroundColor Yellow
    & .\update-db.ps1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Application is running successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🌐 Application URLs:" -ForegroundColor Cyan
        Write-Host "   Blazor App:  http://localhost:5000" -ForegroundColor White
        Write-Host "   pgAdmin:     http://localhost:8080" -ForegroundColor White
        Write-Host ""
        Write-Host "🐘 Database Connection Details:" -ForegroundColor Cyan
        Write-Host "   Host: localhost" -ForegroundColor White
        Write-Host "   Port: 5434" -ForegroundColor White
        Write-Host "   Database: database" -ForegroundColor White
        Write-Host "   Username: user" -ForegroundColor White
        Write-Host "   Password: password" -ForegroundColor White
        Write-Host ""
        Write-Host "🔑 pgAdmin Login Credentials:" -ForegroundColor Cyan
        Write-Host "   Email: admin@admin.com" -ForegroundColor White
        Write-Host "   Password: admin" -ForegroundColor White
        Write-Host ""
        Write-Host "📋 Useful Commands:" -ForegroundColor Gray
        Write-Host "   View Blazor logs:    docker-compose logs -f blazorapp" -ForegroundColor Gray
        Write-Host "   View PostgreSQL logs: docker-compose logs -f pgdb" -ForegroundColor Gray
        Write-Host "   View pgAdmin logs:    docker-compose logs -f pgadmin" -ForegroundColor Gray
        Write-Host ""
        Write-Host "💡 To connect from pgAdmin to PostgreSQL:" -ForegroundColor Yellow
        Write-Host "   1. Open http://localhost:8080" -ForegroundColor Gray
        Write-Host "   2. Login with admin@admin.com / admin" -ForegroundColor Gray
        Write-Host "   3. Right-click 'Servers' → Create → Server" -ForegroundColor Gray
        Write-Host "   4. General tab: Name = 'BlazorAuthApp DB'" -ForegroundColor Gray
        Write-Host "   5. Connection tab:" -ForegroundColor Gray
        Write-Host "      Host: pgdb" -ForegroundColor Gray
        Write-Host "      Port: 5432" -ForegroundColor Gray
        Write-Host "      Database: database" -ForegroundColor Gray
        Write-Host "      Username: user" -ForegroundColor Gray
        Write-Host "      Password: password" -ForegroundColor Gray
    } else {
        Write-Host "❌ Database migration failed!" -ForegroundColor Red
    }
} else {
    Write-Host "❌ PostgreSQL failed to start within $retries attempts" -ForegroundColor Red
    Write-Host "📋 Full PostgreSQL logs:" -ForegroundColor Yellow
    docker-compose logs pgdb
    Write-Host ""
    Write-Host "💡 Troubleshooting suggestions:" -ForegroundColor Yellow
    Write-Host "1. Check Docker Desktop is running properly" -ForegroundColor Gray
    Write-Host "2. Try running: .\cleanup.ps1" -ForegroundColor Gray
    Write-Host "3. Ensure ports 5434 and 8080 are not in use" -ForegroundColor Gray
    Write-Host "4. Check available disk space" -ForegroundColor Gray
    exit 1
}