# Fundoo Notes Microservices

## Overview

Fundoo Notes Microservices is a scalable backend system built using .NET 8 and a microservices architecture. The application provides core note-taking functionalities such as user management, note creation, labeling, and collaboration. It follows clean architecture principles and uses CQRS for better separation of concerns.

## Architecture

The system is divided into independent services:

* **User Service** – Handles authentication and user management
* **Notes Service** – Manages notes (create, update, delete, archive, etc.)
* **Labels Service** – Handles note categorization using labels
* **Collaborator Service** – Supports sharing notes between users
* **API Gateway (Ocelot)** – Routes client requests to appropriate services

Each service is independently deployable and communicates over HTTP.

## Tech Stack

* .NET 8 (ASP.NET Core Web API)
* Microservices Architecture
* Ocelot API Gateway
* Docker & Docker Compose
* SQL Server
* Redis (for caching)
* MediatR (CQRS pattern)
* Dapper (lightweight ORM)

## Features

* User registration and authentication using JWT
* Create, update, archive, and delete notes
* Add labels to notes
* Collaborate on notes with other users
* API Gateway routing using Ocelot
* Containerized deployment using Docker

## Project Structure

```
src/
  ApiGateway/
  Services/
    UserService.API/
    NotesService.API/
    LabelsService.API/
    CollaboratorService.API/
  SharedLibrary/
```

## Getting Started

### Prerequisites

* Docker
* Docker Compose
* .NET SDK (optional, for local development)

### Run the Application

```bash
docker compose up --build
```

### Access Services

| Service              | URL                           |
| -------------------- | ----------------------------- |
| API Gateway          | http://localhost:7001         |
| User Service         | http://localhost:5001/swagger |
| Notes Service        | http://localhost:5002/swagger |
| Labels Service       | http://localhost:5003/swagger |
| Collaborator Service | http://localhost:5004/swagger |

## API Gateway Routing

All external requests should go through the API Gateway:

* Users → `/users/...`
* Notes → `/notes/...`
* Labels → `/labels/...`
* Collaborators → `/collaborators/...`

## Database

SQL Server is used as the primary database. Each service ensures its own tables are created at startup.

## Caching

Redis is used in the Notes Service for caching frequently accessed data. If Redis is unavailable, the service falls back to in-memory caching.

## Development Notes

* Services use retry logic to handle delayed database startup in Docker environments
* HTTPS redirection is disabled for container compatibility
* Environment variables are used for database configuration inside Docker


