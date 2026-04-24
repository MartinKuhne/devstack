import express from 'express'
import { createServer } from 'http'
import { expressMiddleware } from '@as-integrations/express4'
import { config } from "./config/app.js"
import { connectDatabase, disconnectDatabase } from "./config/database.js"
import { createApolloServer } from "./server.js"
import { corsMiddleware } from "./middleware/cors.middleware.js"
import { errorMiddleware } from "./middleware/error.middleware.js"
import { healthRouter } from "./health/health.router.js"

async function bootstrap() {
  try {
    await connectDatabase()
    const server = await createApolloServer()

    const app = express()
    const httpServer = createServer(app)

    app.use(corsMiddleware)
    app.use('/health', healthRouter)
    app.use(
      '/graphql',
      express.json(),
      expressMiddleware(server, {
        context: async () => ({}),
      }) as unknown as express.RequestHandler,
    )
    app.use(errorMiddleware)

    await new Promise<void>((resolve) => httpServer.listen({ port: config.port }, resolve))
    console.log(`Server ready at http://localhost:${config.port}/graphql`)
    console.log(`Health check at http://localhost:${config.port}/health`)
  } catch (error) {
    console.error('Failed to start server:', error)
    await disconnectDatabase()
    process.exit(1)
  }
}

process.on('SIGINT', async () => {
  console.log('Shutting down...')
  await disconnectDatabase()
  process.exit(0)
})

process.on('SIGTERM', async () => {
  console.log('Shutting down...')
  await disconnectDatabase()
  process.exit(0)
})

bootstrap()
