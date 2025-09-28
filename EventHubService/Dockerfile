# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY EventHubService/EventHubService.csproj EventHubService/
COPY Contracts/Contracts.csproj Contracts/
RUN dotnet restore EventHubService/EventHubService.csproj

# Copy the remaining source code
COPY EventHubService/. EventHubService/
COPY Contracts/. Contracts/
RUN dotnet publish EventHubService/EventHubService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EventHubService.dll"]
