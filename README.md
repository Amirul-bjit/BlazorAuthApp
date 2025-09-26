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
- **Image Upload**: AWS S3 integration for file storage

## Architecture

### Service Layer Architecture
The application follows a clean service-oriented architecture:

```
Services/
├── BlogService.cs          # Core blog operations
├── BlogLikeService.cs      # Like/unlike functionality
├── BlogCommentService.cs   # Comment management
├── CategoryService.cs      # Category operations
├── ImageUploadService.cs   # AWS S3 file upload management
└── Interfaces/             # Service contracts
```

### Key Services

- **BlogService**: Manages blog CRUD operations, filtering, and sorting
- **BlogLikeService**: Handles blog like/unlike functionality
- **BlogCommentService**: Manages blog comments and threading
- **CategoryService**: Category management with pagination
- **ImageUploadService**: AWS S3 file upload and management

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
- **File Storage**: AWS S3
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
- **Ports available**: 8080 (app), 5432 (database)

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

# 2. Configure environment variables
cp .env.example .env
# Edit .env with your AWS credentials and other settings

# 3. Start everything with Docker Compose
docker-compose up --build
```

### Container Architecture Overview

The Docker setup creates a secure multi-container environment with the following architecture:

#### BlazorAuthApp Multi-Container System
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   migration     │───▶│       db        │◀───│     webapp      │
│  (runs once)    │    │  (PostgreSQL)   │    │ (Blazor Server) │
└─────────────────┘    └─────────────────┘    └─────────────────┘
       │                        │                        │
       └────────────────────────┼────────────────────────┘
                                │
                        app-network (Docker Bridge)
                                │
                       postgres_data (Volume)
```

#### Container Details

**1. Database Container (`blazorauthapp-db-1`)**
- **Service Name**: `db`
- **Image**: `postgres:15`
- **Purpose**: PostgreSQL database server
- **Port Mapping**: `5432:5432` (host:container)
- **Network**: `app-network`
- **Volume**: `postgres_data:/var/lib/postgresql/data`
- **Environment Variables**:
  - `POSTGRES_PASSWORD`
  - `POSTGRES_USER` (defaults to postgres)
  - `POSTGRES_DB` (defaults to blazorauth)
- **Health Check**: `pg_isready` command every 10s
- **Lifecycle**: Long-running service

**2. Migration Container (`blazorauthapp-migration-1`)**
- **Service Name**: `migration`
- **Build**: `Dockerfile.migration`
- **Purpose**: Database schema initialization
- **Network**: `app-network`
- **Dependencies**: Waits for db health check to pass
- **Environment Variables**:
  - `ConnectionStrings__DefaultConnection`
  - AWS credentials (for application startup compatibility)
- **Command**: `dotnet ef database update --verbose`
- **Lifecycle**: Runs once, exits after completion (`restart: no`)

**3. Web Application Container (`blazorauthapp-webapp-1`)**
- **Service Name**: `webapp`
- **Build**: `Dockerfile`
- **Purpose**: Blazor Server application
- **Port Mapping**: `8080:8080` (host:container)
- **Network**: `app-network`
- **Dependencies**: Waits for migration to complete successfully
- **Environment Variables**:
  - `ASPNETCORE_URLS=http://+:8080`
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ConnectionStrings__DefaultConnection`
  - AWS credentials and configuration
  - File upload settings (`MAX_FILE_SIZE_BYTES`, `ALLOWED_EXTENSIONS`)
- **Lifecycle**: Long-running service

### Container Startup Sequence

```
1. Network Creation
   └── app-network (bridge) created

2. Volume Creation
   └── postgres_data volume created

3. Database Startup
   ├── db container starts
   ├── PostgreSQL initializes database
   ├── Health check begins (pg_isready every 10s)
   └── Status: HEALTHY

4. Migration Execution
   ├── migration container starts (depends on db health)
   ├── Connects to database
   ├── Runs EF Core migrations
   ├── Exits with code 0 (success)
   └── Status: COMPLETED

5. Web Application Startup
   ├── webapp container starts (depends on migration completion)
   ├── Loads application configuration
   ├── Connects to database
   ├── Initializes AWS services
   └── Status: RUNNING on port 8080
```

### Data Flow Architecture

```
External Request (port 8080)
└── webapp container
    ├── Blazor Server App
    ├── Identity Authentication
    ├── Business Logic Services
    │   ├── BlogService
    │   ├── CategoryService
    │   ├── BlogLikeService
    │   ├── BlogCommentService
    │   └── ImageUploadService
    ├── Database Connection
    │   └── db container (PostgreSQL)
    └── External Services
        └── AWS S3 (image storage)
```

### Container Communication

- **webapp ←→ db**: PostgreSQL protocol, port 5432
- **webapp → AWS S3**: HTTPS, external
- **migration ←→ db**: PostgreSQL protocol, port 5432, temporary
- **Host → webapp**: HTTP, port 8080

### Security Architecture

**Network Isolation:**
- All containers in private `app-network`
- Only webapp exposed to host (port 8080)
- Database accessible only from internal network
- Migration has temporary access to database

**Environment Variables:**
- Sensitive data (.env file → container env vars)
- No hardcoded credentials in images
- AWS credentials passed securely via env vars

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

**Access Points:**
- **Application**: http://localhost:8080
- **Database**: localhost:5432 (for external tools)

#### Production Mode
```bash
# Set environment variables for production
# ASPNETCORE_ENVIRONMENT=Production in .env file
docker-compose up --build -d
```

**Features:**
- Optimized error handling
- Production logging levels
- Security headers enabled
- Performance optimizations

### Container Resource Requirements

**db container:**
- CPU: Moderate (database operations)
- Memory: ~128MB base + query cache
- Disk: Persistent volume for data
- Network: Internal only (except exposed port)

**migration container:**
- CPU: Low (short-lived)
- Memory: ~256MB (.NET SDK + EF tools)
- Disk: Temporary (exits after completion)
- Network: Internal only

**webapp container:**
- CPU: Moderate to High (Blazor Server)
- Memory: ~256-512MB (.NET runtime + app)
- Disk: Application files only
- Network: External port 8080 + internal

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
   - Create database: `CREATE DATABASE blazorauth;`
   - Create user with appropriate permissions

3. **Update Configuration**
   
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=blazorauth;Username=postgres;Password=your_password;Port=5432"
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

## Configuration

### Environment Variables

This project uses environment variables for configuration. Copy `.env.example` to `.env` and fill in your values before running:

```bash
# Database Configuration
POSTGRES_PASSWORD=your_secure_password
POSTGRES_USER=postgres
POSTGRES_DB=blazorauth
DB_CONNECTION_STRING=Host=db;Database=blazorauth;Username=postgres;Password=your_secure_password

# AWS S3 Configuration
AWS_ACCESS_KEY_ID=your_access_key
AWS_SECRET_ACCESS_KEY=your_secret_key
AWS_DEFAULT_REGION=us-east-1
S3_BUCKET_NAME=your_bucket_name

# File Upload Configuration
MAX_FILE_SIZE_BYTES=10485760
ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.gif,.webp,.bmp
PUBLIC_READ_ACCESS=true
```

### Required Variables
- `AWS_ACCESS_KEY_ID` - AWS access key for S3
- `AWS_SECRET_ACCESS_KEY` - AWS secret key for S3
- `AWS_DEFAULT_REGION` - AWS region for S3 bucket
- `S3_BUCKET_NAME` - S3 bucket name for file storage
- `POSTGRES_PASSWORD` - PostgreSQL password
- `DB_CONNECTION_STRING` - Database connection string
- `MAX_FILE_SIZE_BYTES` - Maximum file upload size
- `ALLOWED_EXTENSIONS` - Allowed file extensions
- `PUBLIC_READ_ACCESS` - S3 public read access setting

See `.env.example` for details and default values.

## Database Management

### Container Database Access

#### Using Docker Commands
```bash
# List all tables
docker exec -it blazorauthapp-db-1 psql -U postgres -d blazorauth -c "\dt"

# Access PostgreSQL shell
docker exec -it blazorauthapp-db-1 psql -U postgres -d blazorauth

# Check migrations history
docker exec -it blazorauthapp-db-1 psql -U postgres -d blazorauth -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

#### Database Credentials (Docker Environment)
- **Host**: `db` (internal) or `localhost` (external)
- **Port**: `5432` (both internal and external)
- **Database**: `blazorauth`
- **Username**: `postgres`
- **Password**: Set in `.env` file

## Migration Management

### Automatic Migrations (Docker)

The Docker setup automatically handles database migrations through the `migration` service:

1. **On startup**: Migration service runs first
2. **Checks database**: Compares current schema with migrations
3. **Applies changes**: Runs any pending migrations
4. **Exits successfully**: Allows main application to start
5. **Database ready**: Application connects to updated database

### Manual Migration Operations

#### Check Migration Status
```bash
# View migration logs
docker-compose logs migration

# Check applied migrations
docker exec -it blazorauthapp-db-1 psql -U postgres -d blazorauth -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

#### Create New Migration
```bash
# Local development
dotnet ef migrations add YourMigrationName

# Docker environment
docker-compose run --rm webapp dotnet ef migrations add YourMigrationName --project ./src
```

#### Apply Migrations Manually
```bash
# Docker environment
docker-compose run --rm migration

# Or with verbose output
docker-compose run --rm webapp dotnet ef database update --verbose --project ./src
```

## Docker Commands Reference

### Basic Operations
```bash
# Start all services
docker-compose up --build

# Start in background (detached)
docker-compose up --build -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f                    # All services
docker-compose logs -f webapp             # Blazor app only
docker-compose logs -f db                 # Database only
docker-compose logs -f migration          # Migration service
```

### Management Commands
```bash
# Check container status
docker-compose ps

# Execute commands in containers
docker exec -it blazorauthapp-webapp-1 bash
docker exec -it blazorauthapp-db-1 psql -U postgres -d blazorauth

# View container resource usage
docker stats

# Clean up unused resources
docker system prune -f
```

### Database Operations
```bash
# Manual migration run
docker-compose run --rm migration

# Database backup
docker exec blazorauthapp-db-1 pg_dump -U postgres blazorauth > backup.sql

# Database restore
docker exec -i blazorauthapp-db-1 psql -U postgres -d blazorauth < backup.sql
```

### Troubleshooting Commands
```bash
# View detailed container information
docker inspect blazorauthapp-webapp-1

# Check network connectivity
docker network inspect blazorauthapp_app-network

# Restart specific service
docker-compose restart webapp

# Complete cleanup
docker-compose down --rmi all --volumes --remove-orphans
```

## Production Deployment

### Docker-based Production Deployment

#### Environment Preparation
```bash
# Create production environment file
cat > .env << EOF
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_PASSWORD=your-secure-production-password
AWS_ACCESS_KEY_ID=your-production-access-key
AWS_SECRET_ACCESS_KEY=your-production-secret-key
# ... other production settings
EOF
```

#### Deploy to Production
```bash
# Build and deploy
docker-compose up --build -d

# Monitor logs
docker-compose logs -f
```

### Cloud Deployment Options

- **AWS ECS/EC2**: Container service with RDS PostgreSQL
- **Azure Container Instances**: With Azure Database for PostgreSQL
- **Google Cloud Run**: With Cloud SQL for PostgreSQL

## Monitoring and Logging

### Container Health Monitoring
```bash
# Check container health
docker-compose ps

# View health check logs
docker inspect --format='{{.State.Health}}' blazorauthapp-webapp-1

# Monitor resource usage
docker stats blazorauthapp-webapp-1 blazorauthapp-db-1
```

### Application Logging
```bash
# Real-time application logs
docker-compose logs -f webapp

# Database logs
docker-compose logs -f db

# Export logs to file
docker-compose logs webapp > app-logs.txt
```

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
docker exec blazorauthapp-db-1 pg_isready -U postgres -d blazorauth

# Check database logs
docker-compose logs db

# Test from application container
docker exec blazorauthapp-webapp-1 psql -h db -U postgres -d blazorauth -c "SELECT version();"
```

#### Migration Failures
```bash
# Check migration logs
docker-compose logs migration

# Manual migration run with verbose output
docker-compose run --rm webapp dotnet ef database update --project ./src --verbose

# Reset database (caution: data loss)
docker-compose down --volumes
docker-compose up --build
```

## Contributing

We welcome contributions! Here's how you can help:

### Development Setup for Contributors

#### Using Docker (Recommended)
```bash
git clone https://github.com/Amirul-bjit/BlazorAuthApp.git
cd BlazorAuthApp
cp .env.example .env
# Edit .env with your settings
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
- AWS for S3 storage services

---

**Built with Blazor Server, ASP.NET Core, PostgreSQL, and AWS S3**