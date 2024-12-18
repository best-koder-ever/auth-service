# Use the official .NET image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY AuthService/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY AuthService/. ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "AuthService.dll"]