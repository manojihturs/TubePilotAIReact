# 🗄️ Persistent Database Setup for TubePilotAI

Great news! You're now ready to move from in-memory data storage to a persistent SQL Server database. This means **your data will survive application restarts and be safe for production**.

## 📋 What Changed

| Before | After |
|--------|-------|
| In-memory database (data lost on restart) | SQL Server database (data persists) |
| No external dependencies | Docker-based SQL Server (optional but recommended) |
| Quick testing only | Production-ready storage |

## 🚀 Quick Start (5 minutes)

### Step 1: Install Docker Desktop (if you don't have it)

Download from: https://www.docker.com/products/docker-desktop

After installation, verify it works:
```powershell
docker --version
```

### Step 2: Start SQL Server

From the repository root directory, run:

```powershell
.\setup-database.ps1
```

This automated script will:
✅ Start SQL Server in Docker  
✅ Wait for it to be ready  
✅ Apply database migrations  
✅ Display connection details  

**That's it!** Your database is now running and initialized.

### Step 3: Restart Your Backend

Stop the current backend server and restart it. It will automatically connect to the SQL Server database instead of in-memory storage.

```powershell
cd TubePilotAIReact.Server
dotnet run
```

---

## 📚 Manual Setup (if script doesn't work)

### Start Docker Container Manually

```powershell
# Start SQL Server in Docker
docker-compose -f infra/docker/docker-compose.yml up -d

# Wait 10-30 seconds for SQL Server to initialize...

# Check if it's ready
docker-compose -f infra/docker/docker-compose.yml logs mssql
```

Look for: `SQL Server is now ready for client connections`

### Apply Migrations Manually

```powershell
cd TubePilotAIReact.Server
dotnet ef database update
```

---

## ✅ Verify Everything Works

### Test 1: Create a Prompt Template

1. Open the app in your browser
2. Go to "Prompt Templates"
3. Click "Create New"
4. Add a template and save
5. ✅ Should save successfully

### Test 2: Restart Backend and Verify Data Persists

1. Stop the backend server (Ctrl+C)
2. Restart the backend:
   ```powershell
   cd TubePilotAIReact.Server
   dotnet run
   ```
3. Open the app again
4. Go to "Prompt Templates"
5. ✅ Your template should still be there!

### Test 3: Connect with SQL Server Management Studio (Optional)

If you want to see the data directly:

1. Install SQL Server Management Studio (SSMS) from [here](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
2. Open SSMS
3. Connect with:
   - Server: `localhost,1433`
   - Username: `sa`
   - Password: `P@ssw0rd1234!`
4. ✅ Browse databases → TubePilotAI → Tables → dbo.PromptTemplates

---

## 🔧 Configuration

The database connection is configured in:

**File:** `TubePilotAIReact.Server/appsettings.json`

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost,1433;User Id=sa;Password=P@ssw0rd1234!;Database=TubePilotAI;MultipleActiveResultSets=true;Encrypt=false;"
  }
}
```

**Connection Details:**
- **Server:** localhost,1433 (or 127.0.0.1:1433)
- **Database:** TubePilotAI
- **Username:** sa
- **Password:** P@ssw0rd1234!

---

## 📊 Database Schema

After running migrations, your database includes these tables:

- **PromptTemplates** - Stores your prompt templates
- **AspNetUsers** - User accounts (if authentication is added)
- **AspNetRoles** - User roles (if authentication is added)
- And other supporting tables for the application

---

## 🛑 Useful Commands

### View SQL Server Logs

```powershell
docker-compose -f infra/docker/docker-compose.yml logs -f mssql
```

### Stop SQL Server (Keep Data)

```powershell
docker-compose -f infra/docker/docker-compose.yml down
```

Data persists and will be available when you restart.

### Start SQL Server Again

```powershell
docker-compose -f infra/docker/docker-compose.yml up -d
```

### Remove Everything (⚠️ Deletes Data)

```powershell
docker-compose -f infra/docker/docker-compose.yml down -v
```

Only use this if you want to completely reset the database.

### EF Core Migrations (After Model Changes)

If you modify entity models in the future:

```powershell
cd TubePilotAIReact.Server

# Create a migration
dotnet ef migrations add AddNewFeature

# Apply to database
dotnet ef database update
```

---

## ⚠️ Troubleshooting

### Problem: "Connection refused" or Cannot connect

**Solution:**
1. Verify SQL Server is running: `docker-compose -f infra/docker/docker-compose.yml ps`
2. Check logs: `docker-compose -f infra/docker/docker-compose.yml logs mssql`
3. Wait another 10-20 seconds (SQL Server takes time to start)

### Problem: Port 1433 already in use

**Solution:** Edit `infra/docker/docker-compose.yml` and change:
```yaml
ports:
  - "1434:1433"  # Use 1434 instead
```

Then update `appsettings.json`:
```json
"DefaultConnection": "Server=localhost,1434;..."
```

### Problem: "Login failed for user 'sa'"

**Solution:**
1. Verify password in `docker-compose.yml` matches `appsettings.json`
2. Wait for SQL Server to fully initialize (takes 10-30 seconds)

### Problem: Migrations failed

**Solution:**
1. Ensure SQL Server is running and ready
2. Run with verbose output: `dotnet ef database update --verbose`
3. Check the error message for details

### Problem: "File is locked" when building

**Solution:** Stop the backend server before rebuilding, then restart it.

---

## 📝 Next Steps

1. ✅ Your data is now persistent!
2. ✅ Try creating/editing/deleting prompt templates
3. ✅ Restart the app and verify data persists
4. ✅ (Optional) Connect SSMS to explore the database directly
5. ✅ Consider adding user authentication in the future

---

## 💾 Data Backup

### Backup Your Database

```powershell
# Create a backup
docker exec tubepilot-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "P@ssw0rd1234!" -Q "BACKUP DATABASE [TubePilotAI] TO DISK = N'/var/opt/mssql/backup/TubePilotAI.bak'"

# Copy backup to your machine
docker cp tubepilot-sql:/var/opt/mssql/backup/TubePilotAI.bak ./TubePilotAI.bak
```

### Docker Volume Location

Data is stored in a Docker volume. To find it:

```powershell
docker volume inspect tubepilotaireact_mssql_data
```

---

## 🎉 You're Done!

Your application now has a production-ready persistent database. All data created in prompt templates will survive application restarts, server reboots, and deployments.

**Happy coding! 🚀**

---

**Questions or issues?** Check `DATABASE_SETUP.md` for more detailed information.
