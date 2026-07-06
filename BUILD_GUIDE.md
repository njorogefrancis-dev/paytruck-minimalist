# BUILD & DEPLOYMENT GUIDE

## 🔨 BUILD PROCESS

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio 2022+ (recommended) or VS Code
- 2GB free disk space
- Windows 10+ (for final EXE)

### Building with Batch Script (Windows)
```batch
build.bat
```

What it does:
1. ✅ Restores NuGet packages
2. ✅ Builds solution in Release mode
3. ✅ Runs unit tests
4. ✅ Publishes self-contained executable

Output: `publish\SiteManagerKenya.exe`

### Building with PowerShell Script
```powershell
.\build.ps1 -Configuration Release -Platform x64 -RunTests -Publish
```

Options:
- `-Configuration Release|Debug`
- `-Platform x64|x86`
- `-RunTests` (default: true)
- `-Publish` (default: true)
- `-SkipTests` (skip unit tests)
- `-NoPublish` (skip publish step)

### Building with .NET CLI (Any OS)
```bash
# Restore packages
dotnet restore

# Build
dotnet build -c Release

# Run tests
dotnet test -c Release

# Publish
dotnet publish SiteManagerKenya/SiteManagerKenya.csproj \
  -c Release \
  -r win-x64 \
  --self-contained \
  -p:PublishSingleFile=true \
  -o publish
```

## 🚀 RUNNING THE APPLICATION

### Run with Batch Script (Windows)
```batch
run.bat
```

### Run with PowerShell Script
```powershell
.\run.ps1              # Run published version
.\run.ps1 -Debug       # Run in debug mode
```

### Run Published Executable
```batch
publish\SiteManagerKenya.exe
```

### Run in Debug Mode
```bash
dotnet run --project SiteManagerKenya/SiteManagerKenya.csproj
```

## 📦 BUILD OUTPUT

After successful build, you'll have:

```
publish/
├── SiteManagerKenya.exe          (Main executable - 150-200 MB)
├── SiteManagerKenya.dll
├── Microsoft.*.dll               (Dependencies)
├── sqlite3.dll                   (Database)
└── ...                          (Other runtime files)
```

The EXE is **self-contained** and includes all required .NET runtime.
No need to install .NET SDK on deployment machines.

## 🧪 TESTING

### Run All Tests
```bash
dotnet test -c Release
```

### Run Specific Test
```bash
dotnet test -c Release --filter "PayrollServiceTests"
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

### Expected Test Results
- Total Tests: 15+
- Coverage: 80%+
- Pass Rate: 100%

## 🗄️ DATABASE

### First Run
- Application creates database automatically
- Location: `%APPDATA%\SiteManagerKenya\sitemanager.db`
- Tables created on startup

### Database Migration
1. Back up existing database:
   ```bash
   copy "%APPDATA%\SiteManagerKenya\sitemanager.db" sitemanager.db.backup
   ```

2. Run migration script:
   - Open `Documentation/DATABASE_MIGRATION.sql` in SQLite Studio
   - Execute steps 1-5 to verify
   - Execute steps 6-8 for final migration

3. Verify migration:
   ```sql
   SELECT COUNT(*) FROM TimeRecords;
   ```

## 📊 BUILD TROUBLESHOOTING

### Error: "NuGet sources are not set up correctly"
```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet restore
```

### Error: ".NET SDK version X.X.X required"
```bash
# Check installed version
dotnet --version

# Download required version
# https://dotnet.microsoft.com/download
```

### Error: "Access denied" on build.bat / build.ps1
```powershell
# PowerShell - Set execution policy
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\build.ps1
```

### Build takes too long
```bash
# Clear build cache
dotnet clean
dotnet restore --no-cache
dotnet build -c Release
```

### Tests fail with "SQLite database locked"
- Close any other instances of the application
- Delete database: `%APPDATA%\SiteManagerKenya\sitemanager.db`
- Run tests again

## 🔧 CUSTOM BUILD CONFIGURATION

### Build for 32-bit (x86)
```bash
dotnet publish SiteManagerKenya/SiteManagerKenya.csproj \
  -c Release \
  -r win-x86 \
  --self-contained
```

### Build for Debug (with symbols for debugging)
```bash
dotnet publish SiteManagerKenya/SiteManagerKenya.csproj \
  -c Debug \
  -r win-x64 \
  --self-contained
```

### Build with trimming (smaller file size)
```bash
dotnet publish SiteManagerKenya/SiteManagerKenya.csproj \
  -c Release \
  -r win-x64 \
  --self-contained \
  -p:PublishTrimmed=true
```

## 📈 PERFORMANCE NOTES

- **Build Time:** 30-60 seconds (first build)
- **Rebuild Time:** 5-10 seconds
- **Test Time:** 10-20 seconds
- **Publish Time:** 20-30 seconds
- **Total Build:** ~2 minutes

## ✅ VERIFICATION CHECKLIST

After building, verify:

- [ ] `build.bat` completes without errors
- [ ] `publish\SiteManagerKenya.exe` exists
- [ ] Application starts without errors
- [ ] Database is created in `%APPDATA%\SiteManagerKenya\`
- [ ] Unit tests pass (or skip for now)
- [ ] No console errors on startup
- [ ] Can load workers page
- [ ] Can perform clock in/out
- [ ] Can calculate payroll

## 🚀 DEPLOYMENT

### To Another Machine

1. Copy entire `publish` folder
2. Run `SiteManagerKenya.exe`
3. No .NET SDK installation required!

### To Production

1. Backup production database
2. Copy new EXE to production
3. Run migration script if needed
4. Test all features
5. Monitor logs for issues

## 📞 BUILD SUPPORT

If build fails:

1. Check prerequisites: `.NET 10.0 SDK` installed
2. Run: `dotnet restore`
3. Run: `dotnet clean`
4. Run: `build.bat` again

If problems persist:
1. Check `Documentation/SETUP_INSTRUCTIONS.md`
2. Review code comments
3. Check Visual Studio output window
4. Verify project structure

---

**Ready to build? Run `build.bat` now! 🚀**
