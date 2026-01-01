@echo off
REM ========================================
REM Cosmic Sandbox - Easy Build Script
REM Double-click this file to build the app!
REM ========================================

echo.
echo ==========================================
echo   COSMIC SANDBOX - AUTOMATIC BUILD
echo ==========================================
echo.
echo This will build the Cosmic Sandbox app.
echo Please wait - this takes 2-5 minutes...
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed!
    echo.
    echo Please install .NET 8.0 SDK first:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [1/4] Checking .NET version...
dotnet --version
echo.

echo [2/4] Restoring NuGet packages...
dotnet restore CosmicSandbox.sln
if errorlevel 1 (
    echo ERROR: Failed to restore packages!
    pause
    exit /b 1
)
echo.

echo [3/4] Building the application...
dotnet build CosmicSandbox.sln --configuration Release --no-restore
if errorlevel 1 (
    echo ERROR: Build failed!
    echo Please check the error messages above.
    pause
    exit /b 1
)
echo.

echo [4/4] Publishing the application...
dotnet publish src\CosmicSandbox.App\CosmicSandbox.App.csproj --configuration Release --runtime win-x64 --self-contained true --output publish
if errorlevel 1 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo.

echo ==========================================
echo   BUILD COMPLETE - SUCCESS!
echo ==========================================
echo.
echo Your app is ready to run!
echo.
echo Location: %CD%\publish\CosmicSandbox.exe
echo.
echo Next steps:
echo   1. Go to the 'publish' folder
echo   2. Double-click CosmicSandbox.exe
echo   3. Enjoy!
echo.
echo ==========================================
pause
