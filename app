#!/bin/bash
set -e

case "$1" in
  start)
    echo "🚀 Starting services..."
    dotnet run --project ./UserService/UserService.csproj --urls "http://localhost:5001" &
    dotnet run --project ./ProductService/ProductService.csproj --urls "http://localhost:5003" &
    dotnet run --project ./OrderService/OrderService.csproj --urls "http://localhost:5005" &
    wait
    ;;
  update)
    echo "🔄 Restoring & rebuilding all services..."

    for service in UserService ProductService OrderService; do
      echo "📦 Updating $service..."
      dotnet restore "./$service/$service.csproj"
      dotnet build "./$service/$service.csproj" -c Release
      dotnet ef database update --project "./$service/$service.csproj" || echo "⚠️ No migrations for $service"
    done

    echo "✅ Update finished for all services."
    ;;
  *)
    echo "Usage: ./app {start|update}"
    ;;
esac
