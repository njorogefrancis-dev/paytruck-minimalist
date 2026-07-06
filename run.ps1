# ============================================================================
# ALCHEMIST PAYTRUCK - RUN SCRIPT (PowerShell)
# Runs the application
# ============================================================================

param(
    [switch]$Debug = $false
)

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "ALCHEMIST PAYTRUCK - RUN APPLICATION" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

$publishPath = "publish\SiteManagerKenya.exe"
$projectPath = "SiteManagerKenya\SiteManagerKenya.csproj"

if ($Debug) {
    Write-Host "[Running in DEBUG mode]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Starting application..." -ForegroundColor Cyan
    
    try {
        dotnet run --project "$projectPath"
    }
    catch {
        Write-Host "[✗] ERROR: Failed to run application!" -ForegroundColor Red
        Write-Host "   Please build the project first: .\build.ps1" -ForegroundColor Yellow
        exit 1
    }
}
else {
    # Check if published version exists
    if (Test-Path $publishPath) {
        Write-Host "[Running published version]" -ForegroundColor Green
        Write-Host "Path: $publishPath" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Starting application..." -ForegroundColor Cyan
        
        try {
            & $publishPath
        }
        catch {
            Write-Host "[✗] ERROR: Failed to start application!" -ForegroundColor Red
            Write-Host "   Please rebuild the project: .\build.ps1" -ForegroundColor Yellow
            exit 1
        }
    }
    else {
        Write-Host "[Published version not found, running in debug mode...]" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Starting application..." -ForegroundColor Cyan
        
        try {
            dotnet run --project "$projectPath"
        }
        catch {
            Write-Host "[✗] ERROR: Failed to run application!" -ForegroundColor Red
            Write-Host "   Please build the project first: .\build.ps1" -ForegroundColor Yellow
            exit 1
        }
    }
}

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "[✓] Application closed" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""
