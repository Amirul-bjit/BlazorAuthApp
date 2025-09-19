# BlazorAuthApp

A modern, full-featured blog application built with Blazor Server, ASP.NET Core Identity, and Entity Framework Core. This application provides a complete blogging platform with user authentication, category management, and interactive features.

## Features

### Authentication & Authorization
- User registration and login with ASP.NET Core Identity
- Email confirmation support
- Role-based access control
- External authentication providers support

### Blog Management
- Create, read, update, and delete blog posts
- Rich text editing capabilities
- Blog post publishing/unpublishing
- Author-specific blog management
- Soft delete functionality

### Category System
- Create and manage blog categories
- Category-based blog filtering
- Hierarchical category organization
- Category assignment to blog posts

### Interactive Features
- **Like System**: Users can like/unlike blog posts
- **Comment System**: Threaded commenting on blog posts
- Real-time like and comment counts
- User-specific interaction tracking

### Advanced Features
- **Pagination**: Efficient pagination for all list views
- **Search & Filtering**: Filter blogs by categories and sort options
- **Responsive Design**: Mobile-first responsive UI
- **Soft Delete**: Safe deletion with recovery options

## Architecture

### Service Layer Architecture
The application follows a clean service-oriented architecture:

```
Services/
├── BlogService.cs          # Core blog operations
├── BlogLikeService.cs      # Like/unlike functionality
├── BlogCommentService.cs   # Comment management
├── CategoryService.cs      # Category operations
└── Interfaces/             # Service contracts
```

### Key Services

- **BlogService**: Manages blog CRUD operations, filtering, and sorting
- **BlogLikeService**: Handles blog like/unlike functionality
- **BlogCommentService**: Manages blog comments and threading
- **CategoryService**: Category management with pagination

### Data Layer
- **Entity Framework Core** for data access
- **PostgreSQL** database support (containerized)
- **Code-First** migrations with automatic execution
- **ApplicationDbContext** for database operations

## Technology Stack

- **Frontend**: Blazor Server, Bootstrap 5, Bootstrap Icons
- **Backend**: ASP.NET Core 9.0
- **Authentication**: ASP.NET Core Identity
- **Database**: Entity Framework Core with PostgreSQL
- **UI Components**: Custom Blazor components
- **Styling**: Custom CSS with Bootstrap integration
- **Containerization**: Docker & Docker Compose
- **Database Management**: pgAdmin support

## Getting Started

### Prerequisites

#### For Docker Development (Recommended)
- **Docker** and **Docker Compose** (latest version)
- **Git** for version control
- **4GB+ RAM** available for containers
- **Ports available**: 5000 (app), 5434 (database)

#### For Local Development (Without Docker)
- **.NET 9.0 SDK**
- **PostgreSQL** 15+ (local installation)
- **Visual Studio 2022** or **VS Code**
- **Git** for version control

## Docker Installation & Deployment

### Quick Start with Docker (Recommended)

This is the fastest way to get the application running with all dependencies:

```bash
# 1. Clone the repository
git clone https://github.com/Amirul-bjit/BlazorAuthApp.git
cd BlazorAuthApp

# 2. Start everything with Docker Compose
docker-compose up --build
```

### Docker Architecture Overview

The Docker setup creates a multi-container environment with the following services:

#### Container Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   db-migrate    │───▶│     pgdb        │◀───│   blazorapp     │
│  (runs once)    │    │  (PostgreSQL)   │    │ (Blazor Server) │
└─────────────────┘    └─────────────────┘    └─────────────────┘
       │                        │                        │
       └────────────────────────┼────────────────────────┘
                                │
                        app-network (Docker)
```

#### Service Details

1. **Database Migration Service (`db-migrate`)**
   - **Purpose**: Applies Entity Framework migrations on startup
   - **Base Image**: Uses same Dockerfile as main app
   - **Lifecycle**: Runs once, applies migrations, then exits
   - **Dependencies**: Waits for PostgreSQL to be healthy
   - **Command**: `dotnet ef database update --project ./src`

2. **Blazor Application (`blazorapp`)**
   - **Purpose**: Main web application server
   - **Base Image**: Built from custom Dockerfile
   - **Port Mapping**: Host:5000 → Container:8080
   - **Dependencies**: Waits for database and migration completion
   - **Health Check**: HTTP endpoint monitoring

3. **PostgreSQL Database (`pgdb`)**
   - **Purpose**: Primary data storage
   - **Base Image**: `postgres:15`
   - **Port Mapping**: Host:5434 → Container:5432
   - **Volume**: `pgdata` for persistent storage
   - **Health Check**: `pg_isready` command

### Docker Images Created

#### 1. Main Application Image
- **Name**: `blazorauthapp-blazorapp`
- **Base**: `mcr.microsoft.com/dotnet/sdk:9.0`
- **Size**: ~1.2GB (includes .NET SDK for EF tools)
- **Contents**:
  - Compiled Blazor Server application
  - Entity Framework Core CLI tools
  - Source code for migrations
  - Health check utilities
  - Non-root user (`dotnetuser`)

**Dockerfile Structure:**
```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... build application

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
# ... runtime with EF tools for migrations
```

#### 2. PostgreSQL Database
- **Image**: `postgres:15` (official)
- **Container**: `blazorauthapp-pgdb-1`
- **Size**: ~375MB
- **Features**:
  - UTF-8 encoding by default
  - Health check integration
  - Persistent data volume
  - Custom initialization if needed

#### 3. Migration Service
- **Image**: Same as main application
- **Purpose**: Database schema management
- **Lifecycle**: Runs migrations then exits with code 0
- **Benefits**: Ensures database is ready before app starts

### Development vs Production Modes

#### Development Mode (Default)
```bash
# Start in development mode
docker-compose up --build

# Or detached mode
docker-compose up --build -d
```

**Features:**
- Detailed error pages and debugging info
- Verbose logging output
- Development connection strings
- Hot reload capabilities (when source is mounted)

**Environment Variables:**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__DefaultConnection=Host=pgdb;Port=5432;Database=database;Username=user;Password=password
```

**Access Points:**
- **Application**: http://localhost:5000
- **Database**: localhost:5434 (for external tools like pgAdmin)

#### Production Mode
```bash
# Modify docker-compose.yml environment or use environment file
# Set ASPNETCORE_ENVIRONMENT=Production
docker-compose up --build -d
```

**Features:**
- Optimized error handling
- Production logging levels
- Security headers enabled
- Performance optimizations

**Recommended Production Changes:**
1. **Use environment files for secrets**
2. **Enable HTTPS with reverse proxy**
3. **Use production-grade PostgreSQL settings**
4. **Implement monitoring and logging**

### Container Networking

The Docker setup creates an isolated bridge network (`app-network`):

- **Internal Communication**: Containers communicate using service names
  - `blazorapp` → `pgdb` (database connection)
  - `db-migrate` → `pgdb` (migration connection)
- **External Access**: Only through mapped ports
  - Port 5000: Blazor application
  - Port 5434: PostgreSQL (optional, for external tools)

### Persistent Data Management

#### Volumes
- **`pgdata`**: PostgreSQL data files
  - **Location**: Docker managed volume
  - **Persistence**: Survives container restarts and rebuilds
  - **Backup**: Can be backed up using `docker volume` commands

#### Data Persistence Scenarios
```bash
# Restart containers (data preserved)
docker-compose restart

# Rebuild app (database data preserved)
docker-compose up --build

# Complete reset (data lost)
docker-compose down --volumes
```

### Docker Commands Reference

#### Basic Operations
```bash
# Start all services
docker-compose up --build

# Start in background (detached)
docker-compose up --build -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f                    # All services
docker-compose logs -f blazorapp          # Blazor app only
docker-compose logs -f pgdb               # Database only
docker-compose logs -f db-migrate         # Migration service
```

#### Management Commands
```bash
# Check container status
docker-compose ps

# Execute commands in containers
docker exec -it blazorauthapp-blazorapp-1 bash
docker exec -it blazorauthapp-pgdb-1 psql -U user -d database

# View container resource usage
docker stats

# Clean up unused resources
docker system prune -f
```

#### Database Operations
```bash
# Manual migration run
docker-compose run --rm db-migrate

# Database backup
docker exec blazorauthapp-pgdb-1 pg_dump -U user database > backup.sql

# Database restore
docker exec -i blazorauthapp-pgdb-1 psql -U user -d database < backup.sql
```

#### Troubleshooting Commands
```bash
# View detailed container information
docker inspect blazorauthapp-blazorapp-1

# Check network connectivity
docker network inspect blazorauthapp_app-network

# Restart specific service
docker-compose restart blazorapp
```

### Complete Cleanup
```bash
# Remove containers, networks, and images
docker-compose down --rmi all --remove-orphans

# Remove everything including data volumes
docker-compose down --rmi all --volumes --remove-orphans

# Clean up all Docker resources
docker system prune -a --volumes -f
```

## Local Development (Without Docker)

### Installation Steps

1. **Clone and Setup**
   ```bash
   git clone https://github.com/Amirul-bjit/BlazorAuthApp.git
   cd BlazorAuthApp
   dotnet restore
   ```

2. **Install PostgreSQL**
   - Download from [postgresql.org](https://www.postgresql.org/download/)
   - Create database: `CREATE DATABASE BlazorAuthApp;`
   - Create user with appropriate permissions

3. **Update Configuration**
   
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=BlazorAuthApp;Username=your_user;Password=your_password;Port=5432"
     }
   }
   ```

4. **Install EF Core Tools**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. **Run Migrations**
   ```bash
   dotnet ef database update
   ```

6. **Start Application**
   ```bash
   dotnet run
   ```

7. **Access Application**
   - Navigate to: `https://localhost:7219` or `http://localhost:5269`

### Local Development Benefits
- Faster build times during development
- Native debugging in IDE
- Direct file system access
- Easier code changes and testing

### Local Development Considerations
- Manual PostgreSQL setup and maintenance required
- No built-in database administration tools
- Environment setup complexity
- Dependency management across different systems

## Database Management

### Connecting External pgAdmin

To connect an external pgAdmin installation to your Dockerized PostgreSQL:

1. **Ensure PostgreSQL port is exposed** (already configured in docker-compose.yml)
2. **Install pgAdmin** locally or run in separate Docker container
3. **Create server connection**:
   - **Host**: localhost (or your EC2 public IP if deployed remotely)
   - **Port**: 5434
   - **Database**: database
   - **Username**: user
   - **Password**: password

### Database Credentials

#### Docker Environment
- **Host**: `pgdb` (internal) or `localhost` (external)
- **Port**: `5432` (internal) or `5434` (external)
- **Database**: `database`
- **Username**: `user`
- **Password**: `password`

#### Security Considerations
For production deployments:
- Change default credentials
- Use environment variables for sensitive data
- Restrict database port access
- Enable SSL connections
- Implement regular backup strategies

## Migration Management

### Automatic Migrations (Docker)

The Docker setup automatically handles database migrations through the `db-migrate` service:

1. **On startup**: Migration service runs first
2. **Checks database**: Compares current schema with migrations
3. **Applies changes**: Runs any pending migrations
4. **Exits successfully**: Allows main application to start
5. **Database ready**: Application connects to updated database

### Manual Migration Operations

#### Create New Migration
```bash
# Local development
dotnet ef migrations add YourMigrationName

# Docker environment
docker-compose run --rm blazorapp dotnet ef migrations add YourMigrationName --project ./src
```

#### Apply Migrations
```bash
# Local development
dotnet ef database update

# Docker environment (automatic on container start)
docker-compose run --rm db-migrate
```

#### Rollback Migration
```bash
# Local development
dotnet ef database update PreviousMigrationName

# Docker environment
docker-compose run --rm blazorapp dotnet ef database update PreviousMigrationName --project ./src
```

## Configuration

### Environment Variables

#### Docker Environment
Set in `docker-compose.yml`:
```yaml
services:
  blazorapp:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=pgdb;Port=5432;Database=database;Username=user;Password=password
      - ASPNETCORE_URLS=http://+:8080
```

#### Local Development
Set in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BlazorAuthApp;Username=user;Password=password;Port=5432"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Custom Configuration Options

Override settings using:
1. **Environment Variables**: Use double underscores for nested config (e.g., `ConnectionStrings__DefaultConnection`)
2. **User Secrets**: For development credentials (`dotnet user-secrets`)
3. **appsettings.{Environment}.json**: Environment-specific settings
4. **Docker environment files**: `.env` files for container configuration

## Production Deployment

### Docker-based Production Deployment

#### 1. Environment Preparation
```bash
# Create production environment file
cat > .env.production << EOF
ASPNETCORE_ENVIRONMENT=Production
DB_HOST=your-prod-db-host
DB_PORT=5432
DB_NAME=your-prod-database
DB_USER=your-prod-user
DB_PASSWORD=your-secure-password
EOF
```

#### 2. Production docker-compose.yml
```yaml
version: '3.8'
services:
  blazorapp:
    build: .
    ports:
      - "80:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
    env_file:
      - .env.production
    restart: unless-stopped
    depends_on:
      db-migrate:
        condition: service_completed_successfully

  db-migrate:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
    env_file:
      - .env.production
    entrypoint: ["dotnet", "ef", "database", "update", "--project", "./src"]
    restart: "no"
```

#### 3. Deploy to Production
```bash
# Build and deploy
docker-compose -f docker-compose.prod.yml up --build -d

# Monitor logs
docker-compose -f docker-compose.prod.yml logs -f
```

### Cloud Deployment Options

#### AWS ECS/EC2
1. **Push image to ECR**
2. **Configure ECS service**
3. **Use RDS for PostgreSQL**
4. **Set up Application Load Balancer**

#### Azure Container Instances
1. **Push to Azure Container Registry**
2. **Deploy container group**
3. **Use Azure Database for PostgreSQL**
4. **Configure Application Gateway**

#### Google Cloud Run
1. **Push to Google Container Registry**
2. **Deploy Cloud Run service**
3. **Use Cloud SQL for PostgreSQL**
4. **Configure load balancing**

## Monitoring and Logging

### Container Health Monitoring
```bash
# Check container health
docker-compose ps

# View health check logs
docker inspect --format='{{.State.Health}}' blazorauthapp-blazorapp-1

# Monitor resource usage
docker stats blazorauthapp-blazorapp-1 blazorauthapp-pgdb-1
```

### Application Logging
```bash
# Real-time application logs
docker-compose logs -f blazorapp

# Database logs
docker-compose logs -f pgdb

# Export logs to file
docker-compose logs blazorapp > app-logs.txt
```

### Production Monitoring Setup
- **Application Insights** (Azure)
- **CloudWatch** (AWS)
- **Prometheus + Grafana** (self-hosted)
- **ELK Stack** for log aggregation

## Troubleshooting

### Common Docker Issues

#### Container Startup Problems
```bash
# Check container status
docker-compose ps

# View detailed logs
docker-compose logs service-name

# Inspect container configuration
docker inspect container-name
```

#### Database Connection Issues
```bash
# Test database connectivity
docker exec blazorauthapp-pgdb-1 pg_isready -U user -d database

# Check database logs
docker-compose logs pgdb

# Test from application container
docker exec blazorauthapp-blazorapp-1 psql -h pgdb -U user -d database -c "SELECT version();"
```

#### Migration Failures
```bash
# Check migration logs
docker-compose logs db-migrate

# Manual migration run with verbose output
docker-compose run --rm db-migrate dotnet ef database update --project ./src --verbose

# Reset migrations (caution: data loss)
docker-compose down --volumes
docker-compose up --build
```

#### Port Conflicts
```bash
# Check what's using ports
netstat -tulpn | grep :5000
netstat -tulpn | grep :5434

# Kill conflicting processes or change ports in docker-compose.yml
```

### Performance Issues

#### Container Resource Usage
```bash
# Monitor resource consumption
docker stats

# Increase container resources if needed (modify docker-compose.yml)
deploy:
  resources:
    limits:
      memory: 2G
      cpus: '1.0'
```

#### Database Performance
```bash
# Check database performance
docker exec blazorauthapp-pgdb-1 psql -U user -d database -c "SELECT * FROM pg_stat_activity;"

# Optimize PostgreSQL settings for your workload
```

### Complete System Reset
```bash
# Nuclear option: remove everything and start fresh
docker-compose down --rmi all --volumes --remove-orphans
docker system prune -a --volumes -f
docker-compose up --build
```

## Contributing

We welcome contributions! Here's how you can help:

### Development Setup for Contributors

#### Using Docker (Recommended)
```bash
git clone https://github.com/Amirul-bjit/BlazorAuthApp.git
cd BlazorAuthApp
docker-compose up --build
```

#### Local Development
```bash
git clone https://github.com/Amirul-bjit/BlazorAuthApp.git
cd BlazorAuthApp
dotnet restore
# Setup PostgreSQL and update connection string
dotnet ef database update
dotnet run
```

### Contribution Guidelines

- Follow C# coding conventions
- Write meaningful commit messages
- Add XML documentation for public methods
- Ensure all tests pass before submitting
- Update relevant documentation
- Test in both Docker and local environments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- ASP.NET Core team for the excellent framework
- Blazor community for inspiration and support
- Bootstrap team for the responsive CSS framework
- PostgreSQL community for the robust database
- Docker for containerization platform

---

**Built with Blazor Server, ASP.NET Core, and PostgreSQL**