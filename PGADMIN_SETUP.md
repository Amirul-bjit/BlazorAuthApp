# pgAdmin Setup Guide

## 🚀 Quick Start

1. **Start the application:**
   ```powershell
   .\run.ps1
   ```

2. **Access pgAdmin:**
   - URL: http://localhost:8080
   - Email: admin@admin.com
   - Password: admin

## 🔗 Connect to PostgreSQL Database

### Step 1: Login to pgAdmin
- Open http://localhost:8080 in your browser
- Enter credentials:
  - Email: `admin@admin.com`
  - Password: `admin`

### Step 2: Add Server Connection
1. Right-click on **"Servers"** in the left panel
2. Select **"Create"** → **"Server..."**

### Step 3: Configure Connection
**General Tab:**
- Name: `BlazorAuthApp DB`

**Connection Tab:**
- Host name/address: `pgdb`
- Port: `5432`
- Maintenance database: `database`
- Username: `user`
- Password: `password`

### Step 4: Save and Connect
- Click **"Save"**
- The server should appear in the left panel

## 📊 Exploring Your Database

Once connected, you can:

1. **View Tables:**
   - Expand: BlazorAuthApp DB → Databases → database → Schemas → public → Tables

2. **Common Tables in Your App:**
   - `AspNetUsers` - User accounts
   - `Categories` - Blog categories
   - `Blogs` - Blog posts
   - `BlogLikes` - User likes on blogs
   - `BlogComments` - Comments on blogs

3. **Run Queries:**
   - Right-click on database → Query Tool
   - Example queries:
     ```sql
     -- View all users
     SELECT * FROM "AspNetUsers";
     
     -- View all blog posts
     SELECT * FROM "Blogs";
     
     -- View categories
     SELECT * FROM "Categories";
     ```

## 🔧 Alternative: External pgAdmin Installation

If you prefer to use a locally installed pgAdmin instead of the containerized version:

**Connection Details:**
- Host: `localhost`
- Port: `5434` (external port)
- Database: `database`
- Username: `user`
- Password: `password`

## 📋 Troubleshooting

### pgAdmin won't load
- Check if port 8080 is in use: `netstat -ano | findstr :8080`
- View pgAdmin logs: `docker-compose logs pgadmin`

### Can't connect to PostgreSQL
- Ensure PostgreSQL container is running: `docker-compose ps`
- Check PostgreSQL logs: `docker-compose logs pgdb`

### Container networking issues
- Use container name `pgdb` as host when connecting from pgAdmin container
- Use `localhost:5434` when connecting from external applications