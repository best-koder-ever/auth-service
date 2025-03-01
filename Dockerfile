# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project file and restore dependencies
COPY AuthService/AuthService.csproj AuthService/
RUN dotnet restore AuthService/AuthService.csproj

# Copy the entire project
COPY AuthService/ AuthService/

# Build and publish the application
RUN dotnet publish AuthService/AuthService.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published files
COPY --from=build-env /app/out .

# Expose port 8081
EXPOSE 8081

# Set the entry point for the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
