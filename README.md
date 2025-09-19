# BlazorAuthApp

A modern, full-featured blog application built with Blazor Server, ASP.NET Core Identity, and Entity Framework Core. This application provides a complete blogging platform with user authentication, category management, and interactive features.

## 🚀 Features

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

## 🏗️ Architecture

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

- **[`BlogService`](Services/BlogService.cs)**: Manages blog CRUD operations, filtering, and sorting
- **[`BlogLikeService`](Services/BlogLikeService.cs)**: Handles blog like/unlike functionality
- **[`BlogCommentService`](Services/BlogCommentService.cs)**: Manages blog comments and threading
- **[`CategoryService`](Services/CategoryService.cs)**: Category management with pagination

### Data Layer
- **Entity Framework Core** for data access
- **PostgreSQL** database support (containerized)
- **Code-First** migrations with automatic execution
- **[`ApplicationDbContext`](Data/ApplicationDbContext.cs)** for database operations

## 📱 Pages & Components

### Public Pages
- **Home**: Landing page with featured blogs
- **Blog List**: Paginated blog listing with filtering
- **Blog Details**: Individual blog post view with comments and likes
- **Categories**: Category listing and management

### Authentication Pages
- **Login/Register**: User authentication
- **Account Management**: Profile and security settings
- **External Login**: Third-party authentication support

### Admin Features
- **Category Management**: Create, edit, delete categories
- **Blog Management**: Author dashboard for blog posts
- **User Management**: Administrative user controls

## 🛠️ Technology Stack

- **Frontend**: Blazor Server, Bootstrap 5, Bootstrap Icons
- **Backend**: ASP.NET Core 9.0
- **Authentication**: ASP.NET Core Identity
- **Database**: Entity Framework Core with PostgreSQL
- **UI Components**: Ant Design Blazor
- **Styling**: Custom CSS with Bootstrap integration
- **Containerization**: Docker & Docker Compose
- **Database Management**: pgAdmin (containerized)

## 🚦 Getting Started

### Prerequisites

#### For Docker Development (Recommended)
- **Docker Desktop** (latest version)
- **PowerShell** 5.1 or later (Windows) or **Bash** (Linux/macOS)
- **Git** for version control

#### For Local Development (Without Docker)
- **.NET 9.0 SDK**
- **PostgreSQL** 15+ (local installation)
- **Visual Studio 2022** or **VS Code**
- **Git** for version control

## 🐳 Docker Installation & Deployment

### Quick Start with Docker (Recommended)

This is the fastest way to get the application running with all dependencies:

```powershell
# 1. Clone the repository
git clone https://github.com/yourusername/BlazorAuthApp.git
cd BlazorAuthApp

# 2. Start everything with one command
.\run.ps1
```

The [`run.ps1`](run.ps1) script will:
- ✅ Build the Blazor application Docker image
- ✅ Start PostgreSQL database container
- ✅ Start pgAdmin container for database management
- ✅ Run database migrations automatically
- ✅ Start the Blazor application container
- ✅ Set up networking between containers

### Docker Images Created

The Docker setup creates the following images and containers:

#### 1. **Main Application Image**
- **Name**: `blazorauthapp-blazorapp`
- **Base**: `mcr.microsoft.com/dotnet/sdk:9.0`
- **Size**: ~1.2GB (includes .NET SDK for EF migrations)
- **Contains**: 
  - Blazor Server application
  - Entity Framework Core tools
  - Health check endpoints
  - Source code for migrations

#### 2. **PostgreSQL Database**
- **Image**: `postgres:15`
- **Container**: `blazorauthapp-pgdb-1`
- **Size**: ~375MB
- **Features**:
  - UTF-8 encoding
  - Health checks
  - Persistent data volume

#### 3. **Database Migration Service**
- **Image**: Same as main application
- **Purpose**: Runs EF Core migrations on startup
- **Lifecycle**: Runs once and exits successfully

#### 4. **pgAdmin (Database Management)**
- **Image**: `dpage/pgadmin4:latest`
- **Size**: ~280MB
- **Purpose**: Web-based PostgreSQL administration

### Environment Modes

#### 🔧 Development Mode
```powershell
# Start in development mode (default)
.\run.ps1
```

**Features:**
- Detailed error pages
- Hot reload support
- Development logging
- Debug information

**Access Points:**
- **Application**: http://localhost:5000
- **pgAdmin**: http://localhost:8080 (admin@admin.com / admin)

#### 🚀 Production Mode
```powershell
# Set production environment
$env:ASPNETCORE_ENVIRONMENT = "Production"
.\run.ps1
```

**Features:**
- Optimized performance
- Error handling
- Security headers
- Production logging

### Docker Commands Reference

```powershell
# Start application
.\run.ps1

# View logs
.\logs.ps1                    # Blazor app logs
.\logs.ps1 pgdb              # PostgreSQL logs
.\logs.ps1 pgadmin           # pgAdmin logs

# Update database migrations
.\update-db.ps1

# Stop everything
.\stop.ps1

# Clean restart (removes containers)
.\cleanup.ps1
.\run.ps1

# Complete reset (removes volumes/data)
docker-compose down -v
.\run.ps1
```

### Container Networking

The Docker setup creates an isolated network (`app-network`) where:
- **Blazor App** communicates with **PostgreSQL** via hostname `pgdb`
- **pgAdmin** connects to **PostgreSQL** via hostname `pgdb`
- **External access** via mapped ports

### Persistent Data

Data is stored in Docker volumes:
- **`pgdata`**: PostgreSQL database files (persistent across restarts)
- **Logs**: Container logs (accessible via [`logs.ps1`](logs.ps1))

## 💻 Local Development (Without Docker)

### Installation Steps

1. **Clone and Setup**
   ```bash
   git clone https://github.com/yourusername/BlazorAuthApp.git
   cd BlazorAuthApp
   dotnet restore
   ```

2. **Install PostgreSQL**
   - Download from [postgresql.org](https://www.postgresql.org/download/)
   - Create database: `CREATE DATABASE BlazorAuthApp;`
   - Create user with permissions

3. **Update Configuration**
   
   Update [`appsettings.json`](appsettings.json):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=BlazorAuthApp;Username=your_user;Password=your_password;Port=5432"
     }
   }
   ```

4. **Run Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Start Application**
   ```bash
   dotnet run
   ```

6. **Access Application**
   - Navigate to: `https://localhost:7219` or `http://localhost:5269`

### Local Development Benefits
- ✅ Faster build times
- ✅ Native debugging in IDE
- ✅ Direct file system access
- ✅ Easier code changes

### Local Development Considerations
- ❌ Manual PostgreSQL setup required
- ❌ No pgAdmin included
- ❌ Environment setup complexity
- ❌ Dependency management

## 📊 Data Migration

### From Existing Database

If you have existing data in SQL Server or another database:

```powershell
# 1. Export data from your existing database
.\export-local-data.ps1

# 2. Start Docker containers
.\run.ps1

# 3. Import data to PostgreSQL
.\migrate-data.ps1 -All
```

See [`MIGRATION_GUIDE.md`](MIGRATION_GUIDE.md) for detailed migration instructions.

### Database Management

#### Using pgAdmin (Docker)
1. Open http://localhost:8080
2. Login: `admin@admin.com` / `admin`
3. Add server connection:
   - Host: `pgdb`
   - Port: `5432`
   - Database: `database`
   - Username: `user`
   - Password: `password`

See [`PGADMIN_SETUP.md`](PGADMIN_SETUP.md) for detailed setup guide.

#### Using External Tools
Connect using these credentials:
- **Host**: `localhost`
- **Port**: `5434` (external Docker port)
- **Database**: `database`
- **Username**: `user`
- **Password**: `password`

## 🧪 User Acceptance Testing (UAT)

### Test Scenarios

#### Authentication Tests
- [ ] User can register with valid email and password
- [ ] User can login with correct credentials
- [ ] User cannot access protected pages without authentication
- [ ] Password reset functionality works correctly
- [ ] Email confirmation process is functional

#### Blog Management Tests
- [ ] Authenticated users can create new blog posts
- [ ] Authors can edit their own blog posts
- [ ] Authors can delete their own blog posts
- [ ] Blog posts display correctly with proper formatting
- [ ] Published/unpublished status works correctly

#### Category Management Tests
- [ ] Admin users can create new categories
- [ ] Categories can be edited and updated
- [ ] Categories can be deleted (soft delete)
- [ ] Blog posts can be assigned to categories
- [ ] Category filtering works on blog list

#### Interactive Features Tests
- [ ] Users can like/unlike blog posts
- [ ] Like counts update in real-time
- [ ] Users can add comments to blog posts
- [ ] Comment authors can edit/delete their comments
- [ ] Comment counts display correctly

#### Pagination & Search Tests
- [ ] Pagination works correctly on all list views
- [ ] Page size changes reflect immediately
- [ ] Category filtering shows correct results
- [ ] Blog sorting options work as expected

#### Responsive Design Tests
- [ ] Application works on mobile devices
- [ ] Navigation is accessible on small screens
- [ ] Forms are usable on touch devices
- [ ] All features work across different browsers

## 🔧 Configuration

### Environment Variables

#### Docker Environment
Set in [`docker-compose.yml`](docker-compose.yml):
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Host=pgdb;Port=5432;Database=database;Username=user;Password=password
  - ASPNETCORE_URLS=http://+:8080
```

#### Local Development
Set in [`appsettings.Development.json`](appsettings.Development.json):
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

### Custom Configuration

You can override settings by:
1. **Environment Variables**: Prefix with `BlazorAuthApp_`
2. **User Secrets**: For development credentials
3. **appsettings.{Environment}.json**: Environment-specific settings

## 🚀 Deployment

### Production Deployment with Docker

1. **Build for Production**
   ```bash
   docker build -t blazorauthapp:latest .
   ```

2. **Deploy with Docker Compose**
   ```bash
   # Set production environment
   export ASPNETCORE_ENVIRONMENT=Production
   docker-compose up -d
   ```

3. **Environment-Specific Configuration**
   - Use production connection strings
   - Enable HTTPS redirects
   - Configure authentication providers
   - Set up monitoring and logging

### Cloud Deployment Options

#### Azure Container Instances
- Deploy Docker containers directly
- Use Azure Database for PostgreSQL
- Configure Azure Application Insights

#### AWS ECS/Fargate
- Deploy with AWS ECS
- Use Amazon RDS for PostgreSQL
- Configure CloudWatch logging

#### Kubernetes
- Use provided Dockerfile
- Configure PostgreSQL as StatefulSet
- Set up ingress for HTTPS

## 🤝 Contributing

We welcome contributions from the community! Here's how you can help:

### How to Contribute

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**
4. **Add tests** for new functionality
5. **Commit your changes**
   ```bash
   git commit -m "Add: your feature description"
   ```
6. **Push to your branch**
   ```bash
   git push origin feature/your-feature-name
   ```
7. **Create a Pull Request**

### Development Setup for Contributors

#### Using Docker (Recommended)
```powershell
git clone https://github.com/yourusername/BlazorAuthApp.git
cd BlazorAuthApp
.\run.ps1
```

#### Local Development
```bash
git clone https://github.com/yourusername/BlazorAuthApp.git
cd BlazorAuthApp
dotnet restore
# Setup PostgreSQL and update connection string
dotnet ef database update
dotnet run
```

### Areas for Contribution

- 🐛 **Bug Fixes**: Help us identify and fix bugs
- ✨ **New Features**: Implement new functionality
- 📚 **Documentation**: Improve documentation and examples
- 🎨 **UI/UX**: Enhance the user interface and experience
- 🧪 **Testing**: Add unit tests and integration tests
- 🔧 **Performance**: Optimize application performance
- 🌐 **Accessibility**: Improve accessibility features
- 🐳 **DevOps**: Improve Docker and deployment configurations

### Development Guidelines

- Follow C# coding conventions
- Write meaningful commit messages
- Add XML documentation for public methods
- Ensure all tests pass before submitting
- Update relevant documentation
- Test in both Docker and local environments

### Code Style
- Use meaningful variable and method names
- Follow async/await patterns consistently
- Implement proper error handling
- Add logging where appropriate

## 📋 Troubleshooting

### Docker Issues

#### Containers won't start
```powershell
# Check Docker is running
docker version

# View logs
.\logs.ps1 pgdb
.\logs.ps1 blazorapp

# Clean restart
.\cleanup.ps1
.\run.ps1
```

#### Database connection issues
```powershell
# Check PostgreSQL is ready
docker exec blazorauthapp-pgdb-1 pg_isready -U user -d database

# Restart database migrations
.\update-db.ps1
```

#### Port conflicts
```powershell
# Check what's using ports
netstat -ano | findstr ":5000"
netstat -ano | findstr ":8080"
netstat -ano | findstr ":5434"

# Kill processes or change ports in docker-compose.yml
```

### Local Development Issues

#### Migration errors
```bash
# Reset database
dotnet ef database drop
dotnet ef database update

# Check connection string in appsettings.json
```

#### Package restore issues
```bash
# Clear package cache
dotnet nuget locals all --clear
dotnet restore
```

### Common Solutions

#### Complete reset
```powershell
# Docker environment
.\cleanup.ps1
docker system prune -f
.\run.ps1

# Local environment
dotnet ef database drop
dotnet restore
dotnet ef database update
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Blazor community for inspiration and support
- Bootstrap team for the responsive CSS framework
- Ant Design Blazor for UI components
- PostgreSQL community for the robust database
- Docker for containerization platform

## 📧 Contact

For questions, suggestions, or support, please:
- Open an issue on GitHub
- Contact the maintainers
- Join our community discussions

---

**Built with ❤️ using Blazor Server, ASP.NET Core, and PostgreSQL**