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
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorAuthApp.dll"]