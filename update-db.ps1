Write-Host "🔄 Updating database with Entity Framework migrations..." -ForegroundColor Green

# Ensure PostgreSQL container is running
$pgStatus = docker ps --filter "name=pgdb" --format "{{.Names}}"

if (-not $pgStatus) {
    Write-Host "PostgreSQL container is not running. Starting it..." -ForegroundColor Yellow
    docker-compose up -d pgdb
    
    # Wait for PostgreSQL to be ready
    Write-Host "⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
    $retries = 20
    $ready = $false
    
    for ($i = 1; $i -le $retries; $i++) {
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
        Start-Sleep -Seconds 2
    }
    
    if (-not $ready) {
        Write-Host "❌ PostgreSQL failed to start" -ForegroundColor Red
        exit 1
    }
}

# Get the network name (Docker Compose creates a network based on the folder name)
$networkName = "blazorauthapp_app-network"

# Check if network exists
$networkExists = docker network ls --filter "name=$networkName" --format "{{.Name}}"
if (-not $networkExists) {
    Write-Host "❌ Could not find Docker network: $networkName" -ForegroundColor Red
    Write-Host "Available networks:" -ForegroundColor Gray
    docker network ls
    exit 1
}

Write-Host "Using Docker network: $networkName" -ForegroundColor Gray

# Use the containerized database connection string
$connectionString = "Host=pgdb;Port=5432;Database=database;Username=user;Password=password"

try {
    Write-Host "🔧 Installing EF Core tools and running migrations..." -ForegroundColor Yellow
    
    # Run EF Core migrations using a temporary .NET SDK container with EF tools installed
    docker run --rm `
        -v "${PWD}:/src" `
        -w /src `
        --network $networkName `
        -e "ConnectionStrings__DefaultConnection=$connectionString" `
        mcr.microsoft.com/dotnet/sdk:9.0 `
        bash -c "dotnet tool install --global dotnet-ef --version 9.0.0 && export PATH=`$PATH:/root/.dotnet/tools && dotnet ef database update --verbose"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database migrations applied successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Database migration failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error running database migrations: $_" -ForegroundColor Red
    exit 1
}