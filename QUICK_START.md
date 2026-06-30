# ⚡ Quick Reference - Database Setup

## 🟢 Start Here (2 Steps)

### Step 1: Run Setup Script
```powershell
.\setup-database.ps1
```
**That's it!** The script handles everything.

### Step 2: Restart Backend
```powershell
cd TubePilotAIReact.Server
dotnet run
```

## 📋 Prerequisites Checklist
- [ ] Docker Desktop installed
- [ ] Port 1433 is available
- [ ] Backend is stopped

## 🔗 Connection String
```
Server=localhost,1433;User Id=sa;Password=P@ssw0rd1234!;Database=TubePilotAI;MultipleActiveResultSets=true;Encrypt=false;
```

## 🧪 Quick Tests
```powershell
# Test 1: API is responding
curl http://localhost:5285/api/prompt-templates

# Test 2: Create a prompt template via UI
# Go to app → Prompt Templates → Create New → Save

# Test 3: Restart backend and check data persists
# Stop backend (Ctrl+C) → dotnet run → Check app
```

## 🛠️ Troubleshooting Quick Fixes

| Problem | Solution |
|---------|----------|
| Connection refused | `docker-compose -f infra/docker/docker-compose.yml ps` (check if running) |
| Port 1433 in use | Change port in `docker-compose.yml`: `1434:1433` |
| SQL Server taking too long | Wait 20-30 seconds, containers need initialization |
| Build fails (file locked) | Stop backend server before rebuilding |

## 📞 Get Detailed Help
- Full guide: `DATABASE_SETUP.md`
- User guide: `PERSISTENT_DATABASE_SETUP.md`
- Setup report: `DATABASE_IMPLEMENTATION_COMPLETE.md`

## 🎯 That's All!
Your database is now persistent. **No more data loss on restarts!** 🎉

---
**Need help?** Check the markdown files above for detailed troubleshooting.
