@echo off
echo ==========================================
echo   HRMS Portal -- Full Stack Startup
echo ==========================================
echo.
echo [1/2] Starting HRMS API (port 5000)...
start "HRMS API" cmd /k "cd src\HRMS.Web && dotnet run"

echo Waiting for API to start...
timeout /t 5 /nobreak > nul

echo [2/2] Starting HRMS Web Portal (port 5001)...
start "HRMS Web Portal" cmd /k "cd src\HRMS.WebPortal && dotnet run"

echo.
echo ==========================================
echo   API:    http://localhost:5000/swagger
echo   Portal: http://localhost:5001
echo.
echo   Login:  admin@acmecorp.in / Admin@123
echo ==========================================
timeout /t 3 /nobreak > nul
start http://localhost:5001
