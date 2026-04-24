# Quickstart: GraphQL API for AI Development System (Node.js/Express/Apollo)

**Branch**: `002-graphql-api` | **Date**: 2026-04-24

## Prerequisites

- Node.js 20+ and npm
- Docker and Docker Compose (for PostgreSQL via Testcontainers)
- Git

## Local Development

### 1. Clone and Navigate

```powershell
cd C:\Users\mkuhn\src\devstack
git checkout 002-graphql-api
```

### 2. Install Dependencies

```powershell
npm install --prefix src/graphql
```

### 3. Configure Database

Set the PostgreSQL connection string via environment variable:

```powershell
$env:DATABASE_URL = "postgresql://postgres:postgres@localhost:5432/devstack"
```

For local development, start PostgreSQL with Docker:

```powershell
docker run -d --name devstack-db -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=devstack postgres:16
```

### 4. Run Prisma Migrations

```powershell
npx prisma migrate deploy --schema src/graphql/prisma/schema.prisma
```

Or for development with auto-create:

```powershell
npx prisma migrate dev --schema src/graphql/prisma/schema.prisma
```

### 5. Start the Server

```powershell
npm run dev --prefix src/graphql
```

The server will start at `http://localhost:4000` (or the configured port).

### 6. Access the GraphQL Playground

- GraphQL endpoint: `http://localhost:4000/graphql`
- Health check: `http://localhost:4000/health`

## Running Tests

### Unit Tests

```powershell
npm run test:unit --prefix src/graphql
```

### Integration Tests (with Testcontainers)

```powershell
npm run test:integration --prefix src/graphql
```

Integration tests spin up a real PostgreSQL container for each test run.

### All Tests

```powershell
npm test --prefix src/graphql
```

### Linting

```powershell
npm run lint --prefix src/graphql
```

### Type Checking

```powershell
npm run typecheck --prefix src/graphql
```

## Building the Project

```powershell
npm run build --prefix src/graphql
```

## Docker Build

```powershell
docker build -f src/graphql/Dockerfile -t devstack-graphql-node:latest src/graphql/
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| DATABASE_URL | PostgreSQL connection string | - |
| PORT | Server port | `4000` |
| NODE_ENV | Environment | `development` |
| OTEL_EXPORTER_OTLP_ENDPOINT | OpenTelemetry OTLP endpoint | - |

## GraphQL API Usage

### Example Query

```graphql
query GetProjects {
  projects {
    edges {
      node {
        id
        name
        description
        repository
      }
    }
  }
}
```

### Example Mutation

```graphql
mutation CreateProject($input: CreateProjectInput!) {
  createProject(input: $input) {
    id
    name
    repository
  }
}
```

With variables:

```json
{
  "input": {
    "name": "My Project",
    "repository": "https://github.com/org/repo",
    "description": "A sample project"
  }
}
```

## Troubleshooting

### Database Connection Issues

- Verify PostgreSQL is running: `docker ps | grep postgres`
- Check connection string format: `postgresql://user:password@host:5432/dbname`
- Run migrations: `npx prisma migrate dev`

### Testcontainer Failures

- Ensure Docker daemon is running
- Check available disk space
- Verify Docker network is accessible

### Port Conflicts

- Check if port 4000 is in use: `netstat -ano | findstr :4000`
- Change the port in `src/graphql/.env`
