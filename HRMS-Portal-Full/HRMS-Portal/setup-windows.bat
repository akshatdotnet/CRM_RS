@echo off
echo ==========================================
echo   HRMS Portal - Full Setup (Windows)
echo ==========================================
echo.

dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8 SDK not found.
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    pause & exit /b 1
)
echo [OK] .NET SDK: 
dotnet --version

echo.
echo Restoring NuGet packages...
dotnet restore HRMS.sln
if errorlevel 1 ( echo ERROR: Restore failed. & pause & exit /b 1 )
echo [OK] Packages restored.

echo.
echo Building all projects...
dotnet build HRMS.sln --no-restore -c Debug
if errorlevel 1 ( echo ERROR: Build failed. & pause & exit /b 1 )
echo [OK] Build successful.

echo.
echo ==========================================
echo   Choose startup mode:
echo   1. API only   (http://localhost:5000)
echo   2. Portal + API (recommended)
echo ==========================================
set /p CHOICE="Enter 1 or 2: "

if "%CHOICE%"=="1" (
  cd src\HRMS.Web && dotnet run --no-build
) else (
  call run-all.bat
)
