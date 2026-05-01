#!/bin/bash
echo "=========================================="
echo "  HRMS Portal -- Full Stack Startup"
echo "=========================================="
echo ""

# Start API in background
echo "[1/2] Starting HRMS API (port 5000)..."
cd src/HRMS.Web
dotnet run &
API_PID=$!
cd ../..

echo "Waiting for API to start (5s)..."
sleep 5

# Start Web Portal in foreground
echo "[2/2] Starting HRMS Web Portal (port 5001)..."
echo ""
echo "=========================================="
echo "  API:    http://localhost:5000/swagger"
echo "  Portal: http://localhost:5001"
echo "  Login:  admin@acmecorp.in / Admin@123"
echo "=========================================="
echo ""
cd src/HRMS.WebPortal
dotnet run

# Cleanup
kill $API_PID 2>/dev/null
