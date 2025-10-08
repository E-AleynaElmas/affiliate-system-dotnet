# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY src/AffiliateSystem.API/*.csproj ./src/AffiliateSystem.API/
COPY src/AffiliateSystem.Application/*.csproj ./src/AffiliateSystem.Application/
COPY src/AffiliateSystem.Domain/*.csproj ./src/AffiliateSystem.Domain/
COPY src/AffiliateSystem.Infrastructure/*.csproj ./src/AffiliateSystem.Infrastructure/
COPY tests/AffiliateSystem.Tests/*.csproj ./tests/AffiliateSystem.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/AffiliateSystem.API/AffiliateSystem.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "AffiliateSystem.API.dll"]