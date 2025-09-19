# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY BlazorAuthApp.csproj ./
RUN dotnet restore "BlazorAuthApp.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "BlazorAuthApp.csproj" -c Release -o /app/publish

# Stage 2: Runtime with SDK for EF Tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install EF Core tools globally
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy published app
COPY --from=build /app/publish .

# Copy source files for migrations (needed for dotnet ef)
COPY . ./src/

# Create a non-root user and set permissions
RUN adduser --disabled-password --gecos '' dotnetuser && \
    chown -R dotnetuser /app && \
    chown -R dotnetuser /root/.dotnet

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "BlazorAuthApp.dll"]