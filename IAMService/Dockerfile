# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY IAMService/IAMService.csproj IAMService/
RUN dotnet restore IAMService/IAMService.csproj

COPY IAMService/. IAMService/
RUN dotnet publish IAMService/IAMService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IAMService.dll"]
