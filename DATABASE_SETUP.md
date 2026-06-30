# Database Setup Guide for TubePilotAI

This guide explains how to set up a persistent SQL Server database for TubePilotAI to replace the in-memory database.

## Prerequisites

- Docker Desktop installed ([Download](https://www.docker.com/products/docker-desktop))
- .NET 10 SDK
- PowerShell or Command Prompt

## Quick Setup (Recommended)

### Option 1: Automated Setup Script (Easiest)

Run the provided PowerShell script from the repository root:

```powershell
.\setup-database.ps1
```

This script will:
1. ✅ Start SQL Server in a Docker container
2. ✅ Wait for SQL Server to be ready
3. ✅ Apply EF Core migrations to create the database schema
4. ✅ Display connection details

### Option 2: Manual Setup

#### Step 1: Start SQL Server Container

From the repository root:

```bash
docker-compose -f infra/docker/docker-compose.yml up -d
```

Wait 10-30 seconds for SQL Server to fully start. Check status:

```bash
docker-compose -f infra/docker/docker-compose.yml logs mssql
```

Look for: `SQL Server is now ready for client connections`

#### Step 2: Apply EF Core Migrations

```bash
cd TubePilotAIReact.Server
dotnet ef database update
```

This creates the database schema based on your entity models.

## Connection Details

When using the Docker setup:

- **Server:** localhost,1433 (or 127.0.0.1:1433)
- **Username:** sa
- **Password:** P@ssw0rd1234!
- **Database:** TubePilotAI
- **Connection String:** `Server=localhost,1433;User Id=sa;Password=P@ssw0rd1234!;Database=TubePilotAI;MultipleActiveResultSets=true;Encrypt=false;`

This is already configured in `appsettings.json`.

## Connect SQL Server Management Studio (Optional)

To view/manage the database with SSMS:

1. Open **SQL Server Management Studio**
2. **Server name:** `localhost,1433` or `127.0.0.1:1433`
3. **Authentication:** SQL Server Authentication
4. **Login:** `sa`
5. **Password:** `P@ssw0rd1234!`
6. Click **Connect**

## Data Persistence

Data is now persisted in a Docker volume (`mssql_data`). This means:

- ✅ Data survives container restarts
- ✅ Data is stored on your machine's disk
- ❌ Removing the volume with `docker volume prune` will delete data

## Useful Commands

### Start/Stop Database

```bash
# Start (if stopped)
docker-compose -f infra/docker/docker-compose.yml up -d

# Stop
docker-compose -f infra/docker/docker-compose.yml down

# View logs
docker-compose -f infra/docker/docker-compose.yml logs -f mssql

# Remove everything (⚠️ deletes data)
docker-compose -f infra/docker/docker-compose.yml down -v
```

### EF Core Migrations

```bash
cd TubePilotAIReact.Server

# Create a new migration (after model changes)
dotnet ef migrations add MigrationName -o Persistence/Migrations

# Apply pending migrations
dotnet ef database update

# View migration history
dotnet ef migrations list
```

## Troubleshooting

### "Connection refused" error

- SQL Server container hasn't started yet → Wait 10-30 seconds and retry
- Port 1433 is in use → Change the port in `docker-compose.yml` (e.g., `1434:1433`)
- Docker is not running → Start Docker Desktop

### "Login failed for user 'sa'"

- Verify the password in `docker-compose.yml` matches `appsettings.json`
- Check SQL Server is running: `docker-compose -f infra/docker/docker-compose.yml ps`

### Migrations failed

- Ensure SQL Server is ready: `docker-compose -f infra/docker/docker-compose.yml logs mssql`
- Check connection string in `appsettings.json`
- Try `dotnet ef database update --verbose` for detailed output

### All data lost after restart

- Check that `docker-compose.yml` has the volumes section
- Verify Docker volumes: `docker volume ls | grep mssql`
- Data should persist across container restarts (but not if you ran `docker-compose down -v`)

## Next Steps

After setup:

1. ✅ Backend server automatically uses the persistent database
2. ✅ Create/update/delete prompt templates → data persists
3. ✅ Restart backend → data is still there
4. ✅ Optionally connect SSMS to view data directly

---

**Happy coding! 🚀**

For issues, check the logs: `docker-compose -f infra/docker/docker-compose.yml logs -f`
