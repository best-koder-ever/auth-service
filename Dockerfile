# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project file and restore dependencies
COPY AuthService/AuthService.csproj ./AuthService/
RUN dotnet restore ./AuthService/AuthService.csproj

# Copy the rest of the application files
COPY AuthService/ ./AuthService/

# Build the application
RUN dotnet publish ./AuthService/AuthService.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/AuthService/out .

# Expose port 80
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "AuthService.dll"]
