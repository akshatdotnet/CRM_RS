#!/bin/bash
set -e

echo "=========================================="
echo "  HRMS Portal - Full Setup (Linux/Mac)"
echo "=========================================="

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET 8 SDK not found."
    echo "Install: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
echo "[OK] .NET SDK: $(dotnet --version)"

echo ""
echo "Restoring packages..."
dotnet restore HRMS.sln
echo "[OK] Packages restored."

echo ""
echo "Building solution..."
dotnet build HRMS.sln --no-restore -c Debug
echo "[OK] Build successful."

echo ""
echo "What would you like to start?"
echo "  1 = API only  (http://localhost:5000/swagger)"
echo "  2 = API + Web Portal (recommended)"
read -p "Enter 1 or 2 [2]: " CHOICE
CHOICE=${CHOICE:-2}

if [ "$CHOICE" = "1" ]; then
    cd src/HRMS.Web && dotnet run --no-build
else
    chmod +x run-all.sh && ./run-all.sh
fi
