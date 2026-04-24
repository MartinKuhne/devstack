import { ApolloServer } from '@apollo/server'
import { makeExecutableSchema } from '@graphql-tools/schema'
import pino from 'pino'
import { queryResolvers } from "./resolvers/query.resolvers.js"
import { mutationResolvers } from "./resolvers/mutation.resolvers.js"
import { initOpenTelemetry } from "./config/opentelemetry.js"

const logger = pino({ name: 'graphql-server' })

initOpenTelemetry()

const typeDefs = `
  enum DeliverableType {
    FEATURE
    DEFECT
    MAINTENANCE
  }

  enum DeliverableStatus {
    DRAFT
    PLANNING
    READY
    INPROGRESS
    DONE
    FAILED
    REJECTED
    NEEDSREVIEW
  }

  enum AgentTaskStatus {
    READY
    INPROGRESS
    DONE
    FAILED
    REJECTED
    NEEDSREVIEW
  }

  type Project {
    id: ID!
    name: String!
    description: String
    repository: String!
  }

  type Deliverable {
    id: ID!
    projectId: ID!
    project: Project
    type: DeliverableType!
    title: String!
    status: DeliverableStatus!
    description: String
    acceptanceCriteria: String
    executionPlan: String
    agentFeedback: String
    securityImpact: String
    performanceImpact: String
    testPlan: String
    deploymentPlan: String
    blocking: String
  }

  type AgentTask {
    id: ID!
    projectId: ID!
    deliverableId: ID!
    title: String!
    status: AgentTaskStatus!
    description: String!
    result: String
    errors: String
    commitHash: String
    complexityRating: Int!
    dependsOnAgentTaskId: ID
    promptTokens: Int
    completionTokens: Int
    executionDurationInSeconds: Int
    agent: String
  }

  type LargeLanguageModel {
    id: ID!
    url: String!
    model: String!
    modelAlias: String
    apiKey: String!
    maxComplexity: Int!
    maxConcurrency: Int
  }

  type PageInfo {
    hasNextPage: Boolean!
    hasPreviousPage: Boolean!
    startCursor: String
    endCursor: String
  }

  type PaginatedProject {
    nodes: [Project]
    pageInfo: PageInfo!
    totalCount: Int!
  }

  type PaginatedDeliverable {
    nodes: [Deliverable]
    pageInfo: PageInfo!
    totalCount: Int!
  }

  type PaginatedAgentTask {
    nodes: [AgentTask]
    pageInfo: PageInfo!
    totalCount: Int!
  }

  type PaginatedLargeLanguageModel {
    nodes: [LargeLanguageModel]
    pageInfo: PageInfo!
    totalCount: Int!
  }

  type CleanupTestDataPayload {
    success: Boolean!
    message: String
  }

  input CreateProjectInput {
    name: String!
    repository: String!
    description: String
  }

  input UpdateProjectInput {
    id: ID!
    name: String
    repository: String
    description: String
  }

  input CreateDeliverableInput {
    projectId: ID!
    title: String!
    type: String!
    description: String!
    initialStatus: DeliverableStatus!
    acceptanceCriteria: String
    executionPlan: String
    securityImpact: String
    performanceImpact: String
    testPlan: String
    deploymentPlan: String
  }

  input UpdateDeliverableInput {
    id: ID!
    title: String
    description: String
    acceptanceCriteria: String
    agentFeedback: String
    executionPlan: String
    securityImpact: String
    performanceImpact: String
    testPlan: String
    deploymentPlan: String
    blocking: String
  }

  input CreateAgentTaskInput {
    deliverableId: ID!
    projectId: ID!
    title: String!
    description: String!
    dependsOnAgentTaskId: ID
    complexityRating: Int
  }

  input UpdateAgentTaskInput {
    id: ID!
    title: String
    description: String
    result: String
    errors: String
    commitHash: String
    dependsOnAgentTaskId: ID
    complexityRating: Int
    promptTokens: Int
    completionTokens: Int
    executionDurationInSeconds: Int
    agent: String
  }

  input CreateLargeLanguageModelInput {
    url: String!
    model: String!
    modelAlias: String
    apiKey: String
    maxComplexity: Int
    maxConcurrency: Int
  }

  input UpdateLargeLanguageModelInput {
    id: ID!
    url: String
    model: String
    modelAlias: String
    apiKey: String
    maxComplexity: Int
    maxConcurrency: Int
  }

  type Query {
    project(id: ID!): Project
    projects(first: Int, after: String, filter: ProjectFilter, sort: ProjectSort): PaginatedProject!
    deliverable(id: ID!): Deliverable
    deliverables(projectId: ID, first: Int, after: String, filter: DeliverableFilter, sort: DeliverableSort): PaginatedDeliverable!
    deliverablesCount(projectId: ID, statusFilter: [DeliverableStatus!], typeFilter: [DeliverableType!]): Int
    agentTask(id: ID!): AgentTask
    agentTasks(deliverableId: ID, first: Int, after: String, filter: AgentTaskFilter, sort: AgentTaskSort): PaginatedAgentTask!
    largeLanguageModel(id: ID!): LargeLanguageModel
    largeLanguageModels(first: Int, after: String, filter: LargeLanguageModelFilter, sort: LargeLanguageModelSort): PaginatedLargeLanguageModel!
  }

  type Mutation {
    createProject(input: CreateProjectInput!): Project!
    updateProject(id: ID!, input: UpdateProjectInput!): Project
    deleteProject(id: ID!): Boolean
    createDeliverable(input: CreateDeliverableInput!): Deliverable!
    updateDeliverable(id: ID!, input: UpdateDeliverableInput!): Deliverable
    updateDeliverableStatus(id: ID!, targetStatus: DeliverableStatus!, actor: String): DeliverableStatus
    deleteDeliverable(id: ID!): Boolean
    checkAndMarkDeliverableDone(deliverableId: ID!): Boolean
    createAgentTask(input: CreateAgentTaskInput!): AgentTask!
    updateAgentTask(id: ID!, input: UpdateAgentTaskInput!): AgentTask
    updateAgentTaskStatus(id: ID!, targetStatus: AgentTaskStatus!): AgentTaskStatus
    deleteAgentTask(id: ID!): Boolean
    createLargeLanguageModel(input: CreateLargeLanguageModelInput!): LargeLanguageModel!
    updateLargeLanguageModel(id: ID!, input: UpdateLargeLanguageModelInput!): LargeLanguageModel
    deleteLargeLanguageModel(id: ID!): Boolean
    cleanupTestData: CleanupTestDataPayload!
  }

  input ProjectFilter {
    name: String
    repository: String
  }

  input ProjectSort {
    name: String
    repository: String
  }

  input DeliverableFilter {
    projectId: String
    status: DeliverableStatus
    type: DeliverableType
  }

  input DeliverableSort {
    title: String
    status: String
  }

  input AgentTaskFilter {
    deliverableId: String
    status: AgentTaskStatus
  }

  input AgentTaskSort {
    title: String
    status: String
  }

  input LargeLanguageModelFilter {
    model: String
    url: String
  }

  input LargeLanguageModelSort {
    model: String
    url: String
  }
`

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
