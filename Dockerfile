# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY BlazorAuthApp.csproj ./
RUN dotnet restore "BlazorAuthApp.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "BlazorAuthApp.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "BlazorAuthApp.dll"]