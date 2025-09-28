# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY ProductService/ProductService.csproj ProductService/
COPY Contracts/Contracts.csproj Contracts/
RUN dotnet restore ProductService/ProductService.csproj

# Copy the remaining source code
COPY ProductService/. ProductService/
COPY Contracts/. Contracts/
RUN dotnet publish ProductService/ProductService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
RUN mkdir -p /data

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProductService.dll"]
