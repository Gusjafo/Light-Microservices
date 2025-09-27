#!/bin/bash
set -e

echo "🚀 Starting all services..."

# Start User Service
dotnet run --project ./UserService/UserService.csproj --urls "http://localhost:5001" &
USER_PID=$!
echo "✅ User Service running on http://localhost:5001 (PID $USER_PID)"

# Start Product Service
dotnet run --project ./ProductService/ProductService.csproj --urls "http://localhost:5003" &
PRODUCT_PID=$!
echo "✅ Product Service running on http://localhost:5003 (PID $PRODUCT_PID)"

# Start Order Service
dotnet run --project ./OrderService/OrderService.csproj --urls "http://localhost:5005" &
ORDER_PID=$!
echo "✅ Order Service running on http://localhost:5005 (PID $ORDER_PID)"

echo "🎉 All services are starting..."
echo "Press Ctrl+C to stop them."

# Keep script running so background processes stay alive
wait
