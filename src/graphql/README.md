# GraphQL API for AI Development System

Node.js/Express/Apollo GraphQL server for managing AI-driven development work.

## Setup

1. Copy `.env.example` to `.env` and configure:
   ```
   DATABASE_URL=postgresql://postgres:postgres@localhost:5432/devstack_graphql
   PORT=4000
   NODE_ENV=development
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Generate Prisma client:
   ```
   npm run prisma:generate
   ```

4. Run migrations:
   ```
   npm run prisma:migrate
   ```

## Development

```
npm run dev
```

## Build & Run

```
npm run build
npm start
```

## Testing

```
npm test
npm run test:unit
npm run test:integration
```

## API

- GraphQL endpoint: `http://localhost:4000/graphql`
- Health check: `http://localhost:4000/health`

## Docker

```
docker build -t graphql-server .
docker run -p 4000:4000 --env-file .env graphql-server
```
