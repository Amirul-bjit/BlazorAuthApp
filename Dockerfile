# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY BlazorAuthApp.csproj ./
RUN dotnet restore "BlazorAuthApp.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "BlazorAuthApp.csproj" -c Release -o /app/publish

# Stage 2: Runtime with SDK for EF Tools and AWS CLI
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Update package lists and install required packages
RUN apt-get update && apt-get install -y \
    curl \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Install AWS CLI v2
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" \
    && unzip awscliv2.zip \
    && ./aws/install \
    && rm -rf awscliv2.zip aws/

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

# Switch to non-root user for security
USER dotnetuser

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "BlazorAuthApp.dll"]