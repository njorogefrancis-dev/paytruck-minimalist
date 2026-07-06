@echo off
REM ============================================================================
REM ALCHEMIST PAYTRUCK - BUILD SCRIPT
REM Builds the complete solution for Windows Desktop
REM
REM Usage:
REM   From cmd.exe:      build.bat
REM   From PowerShell:   .\build.bat   (note the leading .\ - PowerShell will
REM                      not run a script from the current folder without it)
REM ============================================================================

setlocal enabledelayedexpansion

echo.
echo ============================================================================
echo ALCHEMIST PAYTRUCK - BUILD SCRIPT
echo ============================================================================
echo.

REM Set variables
REM MSBUILD_PLATFORM is what the SOLUTION build uses (matches SiteManagerKenya.sln,
REM which only maps "Any CPU" -^> Any CPU project config). RID_ARCH is only used
REM later, to pick the self-contained runtime for the PUBLISH step (win-x64).
REM Keeping these separate avoids the previous bug where the build used one
REM platform and the test step implicitly expected another.
set SOLUTION_PATH=SiteManagerKenya.sln
set CONFIG=Release
set MSBUILD_PLATFORM=Any CPU
set RID_ARCH=x64
set OUTPUT_PATH=publish
set TEST_TFM=net10.0-windows
set TEST_DLL=SiteManagerKenya.Tests\bin\%CONFIG%\%TEST_TFM%\SiteManagerKenya.Tests.dll

REM Check if .NET SDK is installed and capture its version
set DOTNET_VERSION=
for /f "delims=" %%v in ('dotnet --version 2^>nul') do set DOTNET_VERSION=%%v
if "%DOTNET_VERSION%"=="" (
    echo ERROR: .NET SDK is not installed!
    echo Please install .NET 10.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Checking prerequisites...
echo       .NET SDK: %DOTNET_VERSION%
echo       Configuration: %CONFIG%
echo       Build Platform: %MSBUILD_PLATFORM%  (publish target: win-%RID_ARCH%)
echo.

REM Step 1: Restore NuGet packages
echo [2/5] Restoring NuGet packages...
dotnet restore "%SOLUTION_PATH%" -p:Platform="%MSBUILD_PLATFORM%"
if errorlevel 1 (
    echo ERROR: Failed to restore NuGet packages!
    pause
    exit /b 1
)
echo        NuGet packages restored
echo.

REM Step 2: Build solution
echo [3/5] Building solution...
dotnet build "%SOLUTION_PATH%" -c %CONFIG% -p:Platform="%MSBUILD_PLATFORM%" --no-restore
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo        Solution built successfully
echo.

REM Step 3: Run tests - run directly against the DLL that the build step just
REM produced, instead of re-resolving the test project (which is what caused
REM "test source file not found" when the build and test steps disagreed on
REM platform/output folder).
echo [4/5] Running unit tests...
if not exist "%TEST_DLL%" (
    echo WARNING: Test assembly not found at:
    echo          %TEST_DLL%
    echo          Skipping tests - the build above may not have produced it,
    echo          or TEST_TFM in this script no longer matches the project.
    echo.
) else (
    dotnet vstest "%TEST_DLL%"
    if errorlevel 1 (
        echo WARNING: Some tests failed!
        echo         You can continue or fix the issues.
        pause
    ) else (
        echo        Tests completed
    )
)
echo.

REM Step 4: Publish for deployment
echo [5/5] Publishing for deployment...
dotnet publish SiteManagerKenya\SiteManagerKenya.csproj -c %CONFIG% -r win-%RID_ARCH% --self-contained -p:PublishSingleFile=true -o %OUTPUT_PATH%
if errorlevel 1 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo        Application published to %OUTPUT_PATH%
echo.

echo ============================================================================
echo BUILD COMPLETE!
echo ============================================================================
echo.
echo Output: %OUTPUT_PATH%\SiteManagerKenya.exe
echo.
echo To run the application:
echo   - Execute: run.bat
echo   - Or double-click: %OUTPUT_PATH%\SiteManagerKenya.exe
echo.
echo To run in debug mode:
echo   - Execute: dotnet run --project SiteManagerKenya\SiteManagerKenya.csproj
echo.
pause
