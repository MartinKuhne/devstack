import { GraphQLEnumType } from 'graphql'

export const DeliverableTypeEnum = new GraphQLEnumType({
  name: 'DeliverableType',
  values: {
    FEATURE: { value: 'FEATURE' },
    DEFECT: { value: 'DEFECT' },
    MAINTENANCE: { value: 'MAINTENANCE' },
  },
})

export const DeliverableStatusEnum = new GraphQLEnumType({
  name: 'DeliverableStatus',
  values: {
    DRAFT: { value: 'DRAFT' },
    PLANNING: { value: 'PLANNING' },
    READY: { value: 'READY' },
    INPROGRESS: { value: 'INPROGRESS' },
    DONE: { value: 'DONE' },
    FAILED: { value: 'FAILED' },
    REJECTED: { value: 'REJECTED' },
    NEEDSREVIEW: { value: 'NEEDSREVIEW' },
  },
})

export const AgentTaskStatusEnum = new GraphQLEnumType({
  name: 'AgentTaskStatus',
  values: {
    READY: { value: 'READY' },
    INPROGRESS: { value: 'INPROGRESS' },
    DONE: { value: 'DONE' },
    FAILED: { value: 'FAILED' },
    REJECTED: { value: 'REJECTED' },
    NEEDSREVIEW: { value: 'NEEDSREVIEW' },
  },
})
