#!/bin/bash

# Migration script for robust database initialization
set -e

echo "🚀 Starting database migration process..."

# Function to check if database is ready
wait_for_db() {
    echo "⏳ Waiting for database to be ready..."
    
    # Extract connection details from connection string
    # This assumes format: "Host=db;Port=5432;Database=blazorauth;Username=postgres;Password=..."
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        echo "Attempt $attempt of $max_attempts..."
        
        if dotnet ef database update --dry-run > /dev/null 2>&1; then
            echo "✅ Database is ready!"
            return 0
        fi
        
        echo "⏳ Database not ready yet, waiting 2 seconds..."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    echo "❌ Database failed to become ready after $max_attempts attempts"
    return 1
}

# Function to run migrations
run_migrations() {
    echo "📊 Checking current migration status..."
    
    # Check if database exists and show pending migrations
    if dotnet ef migrations list --verbose; then
        echo "🔄 Applying pending migrations..."
        dotnet ef database update --verbose
        echo "✅ All migrations applied successfully!"
    else
        echo "❌ Failed to check migrations"
        return 1
    fi
}

# Function to verify migration success
verify_migrations() {
    echo "🔍 Verifying migration success..."
    
    if dotnet ef migrations list --verbose | grep -q "No migrations found"; then
        echo "ℹ️  No migrations found - this is normal for a fresh project"
        return 0
    fi
    
    # Check if there are any pending migrations
    if dotnet ef database update --dry-run --verbose | grep -q "No pending model changes"; then
        echo "✅ All migrations are up to date!"
        return 0
    else
        echo "⚠️  There might be pending migrations"
        dotnet ef database update --dry-run --verbose
    fi
}

# Main execution
main() {
    echo "🎯 Starting migration process for BlazorAuthApp..."
    
    # Wait for database to be ready
    if ! wait_for_db; then
        echo "❌ Migration failed: Database not accessible"
        exit 1
    fi
    
    # Run migrations
    if ! run_migrations; then
        echo "❌ Migration failed: Could not apply migrations"
        exit 1
    fi
    
    # Verify success
    if ! verify_migrations; then
        echo "⚠️  Migration completed with warnings"
        exit 0
    fi
    
    echo "🎉 Database migration completed successfully!"
    echo "🚀 Application is ready to start!"
}

# Execute main function
main