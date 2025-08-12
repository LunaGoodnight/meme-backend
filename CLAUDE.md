# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

**Local Development:**
```bash
# Run the application locally
dotnet run --project MemeService/MemeService.csproj

# Build the project
dotnet build MemeService/MemeService.csproj

# Create and apply database migrations
dotnet ef migrations add <MigrationName> --project MemeService
dotnet ef database update --project MemeService
```

**Docker Development:**
```bash
# Run with Docker Compose (includes MySQL database)
docker compose up --build

# Access API at http://localhost:5001
# MySQL accessible at localhost:3307
```

## Architecture Overview

This is a .NET 9 ASP.NET Core Web API for meme image CRUD operations with the following key components:

**Data Layer:**
- Entity Framework Core with MySQL via Pomelo provider
- `MemeContext` handles database operations for the `Meme` entity
- Database connection configured through `ConnectionStrings:DefaultConnection` in appsettings

**API Layer:**
- RESTful API with `MemesController` providing full CRUD operations
- Routes follow `/api/memes` pattern with standard HTTP verbs
- Model validation using DataAnnotations on the `Meme` model

**External Services:**
- AWS S3 integration for image storage (configured for DigitalOcean Spaces)
- S3 credentials and service URL configured via `AWS` section in appsettings

**Key Configuration:**
- Database connection string format: `server=host;database=MemeDB;user=username;password=password`
- AWS S3 configuration includes AccessKey, SecretKey, and ServiceURL
- Docker Compose provides complete development environment with MySQL

**Model Structure:**
The `Meme` entity includes validation for ImageUrl (required, valid URL, max 2000 chars) and Keywords (max 10 items).

## Important Notes

- Entity Framework is configured to use MySQL Server version 8.0.21
- Docker Compose uses different database credentials than local development
- S3 client is configured for DigitalOcean Spaces with force path style enabled
- Application runs on ports 5073 (HTTP) and 7205 (HTTPS) in development, port 5001 in Docker