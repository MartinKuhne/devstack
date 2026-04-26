import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { dirname, join } from 'node:path'
import { ApolloServer } from '@apollo/server'
import { makeExecutableSchema } from '@graphql-tools/schema'
import pino from 'pino'
import { queryResolvers } from "./resolvers/query.resolvers.js"
import { mutationResolvers } from "./resolvers/mutation.resolvers.js"
import { initOpenTelemetry } from "./config/opentelemetry.js"

const logger = pino({ name: 'graphql-server' })

initOpenTelemetry()

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

const typeDefs = readFileSync(join(__dirname, 'schema.graphql'), 'utf-8')

const resolvers = {
  DeliverableType: {
    FEATURE: 'FEATURE',
    DEFECT: 'DEFECT',
    MAINTENANCE: 'MAINTENANCE',
  },
  DeliverableStatus: {
    DRAFT: 'DRAFT',
    PLANNING: 'PLANNING',
    READY: 'READY',
    INPROGRESS: 'INPROGRESS',
    DONE: 'DONE',
    FAILED: 'FAILED',
    REJECTED: 'REJECTED',
    NEEDSREVIEW: 'NEEDSREVIEW',
  },
  AgentTaskStatus: {
    READY: 'READY',
    INPROGRESS: 'INPROGRESS',
    DONE: 'DONE',
    FAILED: 'FAILED',
    REJECTED: 'REJECTED',
    NEEDSREVIEW: 'NEEDSREVIEW',
  },
  Query: queryResolvers,
  Mutation: mutationResolvers,
}

export async function createApolloServer(): Promise<ApolloServer> {
  const schema = makeExecutableSchema({ typeDefs, resolvers })

  const server = new ApolloServer({
    schema,
    introspection: true,
    plugins: [
      {
        async requestDidStart() {
          return {
            async willSendResponse() {
              // Logging handled by Pino middleware
            },
          }
        },
      },
    ],
  })

  await server.start()
  logger.info('Apollo Server ready')
  return server
}
