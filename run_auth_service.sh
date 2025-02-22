#!/bin/bash

# Stop and remove the existing container
docker stop auth-service
docker rm auth-service

# Remove the Docker image
docker rmi auth-service

# Remove unused Docker volumes and networks
docker volume prune -f
docker network prune -f

# Navigate to the project directory
cd ~/development/DatingApp/auth-service/

# Rebuild the Docker image
docker build -t auth-service .

# Run the Docker container with the necessary environment variables
docker run -d -p 8081:8081 --name auth-service \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_HTTP_PORTS=8081 \
  -e ASPNETCORE_HTTPS_PORTS=8081 \
  -e ASPNETCORE_URLS=http://*:8081 \
  auth-service

# Check the logs to ensure the service is running correctly
docker logs auth-service
