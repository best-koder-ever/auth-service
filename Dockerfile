# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project file and restore dependencies
COPY AuthService.csproj .
RUN dotnet restore AuthService.csproj

# Copy the entire project
COPY . .
# COPY src/AuthService.Tests/ src/AuthService.Tests/ # Assuming tests might be needed in build - Temporarily commented out

# Build and publish the application
RUN dotnet publish AuthService.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published files
COPY --from=build-env /app/out .

# Copy the private key
COPY private.key /app/private.key

# Expose port 8081
EXPOSE 8081

# Set the entry point for the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
