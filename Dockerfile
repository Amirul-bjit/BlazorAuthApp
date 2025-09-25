# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["BlazorAuthApp.csproj", "./"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime with SDK for EF Tools and AWS CLI
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Install AWS CLI for S3 operations (if needed)
RUN apt-get update && \
    apt-get install -y curl unzip && \
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
    unzip awscliv2.zip && \
    ./aws/install && \
    apt-get remove -y curl unzip && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* awscliv2.zip aws

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "BlazorAuthApp.dll"]