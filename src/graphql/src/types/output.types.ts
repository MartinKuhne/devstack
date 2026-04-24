import {
  GraphQLObjectType,
  GraphQLID,
  GraphQLString,
  GraphQLInt,
  GraphQLNonNull,
  GraphQLList,
  GraphQLBoolean,
} from 'graphql'
import { DeliverableStatusEnum, DeliverableTypeEnum, AgentTaskStatusEnum } from "./enum.types.js"

export const ProjectType = new GraphQLObjectType({
  name: 'Project',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    name: { type: new GraphQLNonNull(GraphQLString) },
    description: { type: GraphQLString },
    repository: { type: new GraphQLNonNull(GraphQLString) },
  },
})

export const DeliverableType = new GraphQLObjectType({
  name: 'Deliverable',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    projectId: { type: new GraphQLNonNull(GraphQLID) },
    project: { type: ProjectType },
    type: { type: new GraphQLNonNull(DeliverableTypeEnum) },
    title: { type: new GraphQLNonNull(GraphQLString) },
    status: { type: new GraphQLNonNull(DeliverableStatusEnum) },
    description: { type: GraphQLString },
    acceptanceCriteria: { type: GraphQLString },
    executionPlan: { type: GraphQLString },
    agentFeedback: { type: GraphQLString },
    securityImpact: { type: GraphQLString },
    performanceImpact: { type: GraphQLString },
    testPlan: { type: GraphQLString },
    deploymentPlan: { type: GraphQLString },
    blocking: { type: GraphQLString },
  },
})

export const AgentTaskType = new GraphQLObjectType({
  name: 'AgentTask',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    projectId: { type: new GraphQLNonNull(GraphQLID) },
    deliverableId: { type: new GraphQLNonNull(GraphQLID) },
    title: { type: new GraphQLNonNull(GraphQLString) },
    status: { type: new GraphQLNonNull(AgentTaskStatusEnum) },
    description: { type: new GraphQLNonNull(GraphQLString) },
    result: { type: GraphQLString },
    errors: { type: GraphQLString },
    commitHash: { type: GraphQLString },
    complexityRating: { type: new GraphQLNonNull(GraphQLInt) },
    dependsOnAgentTaskId: { type: GraphQLID },
    promptTokens: { type: GraphQLInt },
    completionTokens: { type: GraphQLInt },
    executionDurationInSeconds: { type: GraphQLInt },
    agent: { type: GraphQLString },
  },
})

export const LargeLanguageModelType = new GraphQLObjectType({
  name: 'LargeLanguageModel',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    url: { type: new GraphQLNonNull(GraphQLString) },
    model: { type: new GraphQLNonNull(GraphQLString) },
    modelAlias: { type: GraphQLString },
    apiKey: { type: new GraphQLNonNull(GraphQLString) },
    maxComplexity: { type: new GraphQLNonNull(GraphQLInt) },
    maxConcurrency: { type: GraphQLInt },
  },
})

export const PageInfoType = new GraphQLObjectType({
  name: 'PageInfo',
  fields: {
    hasNextPage: { type: new GraphQLNonNull(GraphQLBoolean) },
    hasPreviousPage: { type: new GraphQLNonNull(GraphQLBoolean) },
    startCursor: { type: GraphQLString },
    endCursor: { type: GraphQLString },
  },
})

export const PaginatedProjectType = new GraphQLObjectType({
  name: 'PaginatedProject',
  fields: {
    nodes: { type: new GraphQLList(ProjectType) },
    pageInfo: { type: new GraphQLNonNull(PageInfoType) },
    totalCount: { type: new GraphQLNonNull(GraphQLInt) },
  },
})

export const PaginatedDeliverableType = new GraphQLObjectType({
  name: 'PaginatedDeliverable',
  fields: {
    nodes: { type: new GraphQLList(DeliverableType) },
    pageInfo: { type: new GraphQLNonNull(PageInfoType) },
    totalCount: { type: new GraphQLNonNull(GraphQLInt) },
  },
})

export const PaginatedAgentTaskType = new GraphQLObjectType({
  name: 'PaginatedAgentTask',
  fields: {
    nodes: { type: new GraphQLList(AgentTaskType) },
    pageInfo: { type: new GraphQLNonNull(PageInfoType) },
    totalCount: { type: new GraphQLNonNull(GraphQLInt) },
  },
})

export const PaginatedLargeLanguageModelType = new GraphQLObjectType({
  name: 'PaginatedLargeLanguageModel',
  fields: {
    nodes: { type: new GraphQLList(LargeLanguageModelType) },
    pageInfo: { type: new GraphQLNonNull(PageInfoType) },
    totalCount: { type: new GraphQLNonNull(GraphQLInt) },
  },
})

export const CleanupTestDataPayloadType = new GraphQLObjectType({
  name: 'CleanupTestDataPayload',
  fields: {
    success: { type: new GraphQLNonNull(GraphQLBoolean) },
    message: { type: GraphQLString },
  },
})
