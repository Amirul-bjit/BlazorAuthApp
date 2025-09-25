# Data Migration Quick Start Guide

## 📋 Available Scripts

- `export-local-data.ps1` - Export data from your local SQL Server database
- `migrate-data.ps1` - Import CSV data to containerized PostgreSQL
- `cleanup.ps1` - Clean up Docker containers and networks
- `run.ps1` - Start the containerized application

## 🚀 Quick Migration (If you have CSV files ready)

If you already have CSV files in the `old-bu` folder:

```powershell
# 1. Start the application (if not already running)
.\run.ps1

# 2. Import all data from old-bu folder
.\migrate-data.ps1 -All

# 3. Or import specific tables
.\migrate-data.ps1 -Users
```

## 📊 Full Migration Process

### Step 1: Export from Local Database

```powershell
# Export all tables from your local SQL Server database to old-bu folder
.\export-local-data.ps1

# Or specify custom output directory
.\export-local-data.ps1 -OutputDirectory ".\old-bu"

# Or with a different connection string
.\export-local-data.ps1 -LocalConnectionString "Server=MYSERVER;Database=BlazorAuthApp;Trusted_Connection=true;"
```

### Step 2: Import to PostgreSQL

```powershell
# Make sure your containerized app is running
.\run.ps1

# Import all data from old-bu folder
.\migrate-data.ps1 -All

# Or import specific tables
.\migrate-data.ps1 -Users
.\migrate-data.ps1 -Categories
.\migrate-data.ps1 -Blogs

# Or import from custom directory
.\migrate-data.ps1 -All -ImportDirectory ".\old-bu"
```

### Step 3: Verify Data

- Open pgAdmin: http://localhost:8080
- Login: admin@admin.com / admin
- Connect to database using host: pgdb, port: 5432
- Check your imported data

## 🔧 Troubleshooting

### If export fails:
```powershell
# Check your connection string in export-local-data.ps1
# Make sure SQL Server is running
# Verify database name and server instance
```

### If import fails:
```powershell
# Make sure PostgreSQL container is running
docker-compose ps

# Check PostgreSQL logs
docker-compose logs pgdb

# Restart if needed
.\cleanup.ps1
.\run.ps1
```

### Clean slate:
```powershell
# Complete cleanup and restart
.\cleanup.ps1
.\run.ps1
```

## 📁 File Structure After Export

```
BlazorAuthApp/
├── old-bu/
│   ├── AspNetUsers.csv
│   ├── AspNetUsers.sql
│   ├── Categories.csv
│   ├── Categories.sql
│   ├── Blogs.csv
│   ├── Blogs.sql
│   └── ...
├── export-local-data.ps1
├── migrate-data.ps1
├── cleanup.ps1
└── run.ps1
```

## 🎯 Common Use Cases

### Import just users:
```powershell
.\migrate-data.ps1 -Users
```

### Import from old-bu folder:
```powershell
.\migrate-data.ps1 -All
```

### Import specific CSV file:
```powershell
.\migrate-data.ps1 -CsvPath ".\old-bu\AspNetUsers.csv" -Users
```

### Complete migration:
```powershell
.\export-local-data.ps1
.\migrate-data.ps1 -All
```

## ⚡ Connection Details

**Local SQL Server (source):**
- Server: (localdb)\mssqllocaldb
- Database: BlazorAuthApp
- Authentication: Windows

**PostgreSQL (destination):**
- Host: localhost
- Port: 5434 (external) / 5432 (internal)
- Database: database
- Username: user
- Password: password

## Docker Environment Setup

- Copy `.env.example` to `.env.docker` and fill in your secrets.
- Do NOT commit `.env.docker` to source control.
- All sensitive configuration (AWS, DB, etc.) is injected via environment variables.