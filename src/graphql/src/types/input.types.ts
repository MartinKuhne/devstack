import { GraphQLInputObjectType, GraphQLString, GraphQLID, GraphQLInt, GraphQLNonNull } from 'graphql'
import { DeliverableStatusEnum, DeliverableTypeEnum, AgentTaskStatusEnum } from "./enum.types.js"

export const CreateProjectInput = new GraphQLInputObjectType({
  name: 'CreateProjectInput',
  fields: {
    name: { type: new GraphQLNonNull(GraphQLString) },
    repository: { type: new GraphQLNonNull(GraphQLString) },
    description: { type: GraphQLString },
  },
})

export const UpdateProjectInput = new GraphQLInputObjectType({
  name: 'UpdateProjectInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    name: { type: GraphQLString },
    repository: { type: GraphQLString },
    description: { type: GraphQLString },
  },
})

export const CreateDeliverableInput = new GraphQLInputObjectType({
  name: 'CreateDeliverableInput',
  fields: {
    projectId: { type: new GraphQLNonNull(GraphQLID) },
    title: { type: new GraphQLNonNull(GraphQLString) },
    type: { type: new GraphQLNonNull(DeliverableTypeEnum) },
    description: { type: new GraphQLNonNull(GraphQLString) },
    initialStatus: { type: new GraphQLNonNull(DeliverableStatusEnum) },
    acceptanceCriteria: { type: GraphQLString },
    executionPlan: { type: GraphQLString },
    securityImpact: { type: GraphQLString },
    performanceImpact: { type: GraphQLString },
    testPlan: { type: GraphQLString },
    deploymentPlan: { type: GraphQLString },
  },
})

export const UpdateDeliverableInput = new GraphQLInputObjectType({
  name: 'UpdateDeliverableInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    title: { type: GraphQLString },
    description: { type: GraphQLString },
    acceptanceCriteria: { type: GraphQLString },
    agentFeedback: { type: GraphQLString },
    executionPlan: { type: GraphQLString },
    securityImpact: { type: GraphQLString },
    performanceImpact: { type: GraphQLString },
    testPlan: { type: GraphQLString },
    deploymentPlan: { type: GraphQLString },
    blocking: { type: GraphQLString },
  },
})

export const UpdateDeliverableStatusInput = new GraphQLInputObjectType({
  name: 'UpdateDeliverableStatusInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    targetStatus: { type: new GraphQLNonNull(DeliverableStatusEnum) },
    actor: { type: GraphQLString },
  },
})

export const CreateAgentTaskInput = new GraphQLInputObjectType({
  name: 'CreateAgentTaskInput',
  fields: {
    deliverableId: { type: new GraphQLNonNull(GraphQLID) },
    projectId: { type: new GraphQLNonNull(GraphQLID) },
    title: { type: new GraphQLNonNull(GraphQLString) },
    description: { type: new GraphQLNonNull(GraphQLString) },
    dependsOnAgentTaskId: { type: GraphQLID },
    complexityRating: { type: GraphQLInt },
  },
})

export const UpdateAgentTaskInput = new GraphQLInputObjectType({
  name: 'UpdateAgentTaskInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    title: { type: GraphQLString },
    description: { type: GraphQLString },
    result: { type: GraphQLString },
    errors: { type: GraphQLString },
    commitHash: { type: GraphQLString },
    dependsOnAgentTaskId: { type: GraphQLID },
    complexityRating: { type: GraphQLInt },
    promptTokens: { type: GraphQLInt },
    completionTokens: { type: GraphQLInt },
    executionDurationInSeconds: { type: GraphQLInt },
    agent: { type: GraphQLString },
  },
})

export const UpdateAgentTaskStatusInput = new GraphQLInputObjectType({
  name: 'UpdateAgentTaskStatusInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    targetStatus: { type: new GraphQLNonNull(AgentTaskStatusEnum) },
  },
})

export const CreateLargeLanguageModelInput = new GraphQLInputObjectType({
  name: 'CreateLargeLanguageModelInput',
  fields: {
    url: { type: new GraphQLNonNull(GraphQLString) },
    model: { type: new GraphQLNonNull(GraphQLString) },
    modelAlias: { type: GraphQLString },
    apiKey: { type: GraphQLString },
    maxComplexity: { type: GraphQLInt },
    maxConcurrency: { type: GraphQLInt },
  },
})

export const UpdateLargeLanguageModelInput = new GraphQLInputObjectType({
  name: 'UpdateLargeLanguageModelInput',
  fields: {
    id: { type: new GraphQLNonNull(GraphQLID) },
    url: { type: GraphQLString },
    model: { type: GraphQLString },
    modelAlias: { type: GraphQLString },
    apiKey: { type: GraphQLString },
    maxComplexity: { type: GraphQLInt },
    maxConcurrency: { type: GraphQLInt },
  },
})
