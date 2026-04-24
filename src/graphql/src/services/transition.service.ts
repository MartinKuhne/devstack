import { DeliverableStatus, AgentTaskStatus } from '@prisma/client'

const DELIVERABLE_TRANSITIONS: Record<DeliverableStatus, DeliverableStatus[]> = {
  DRAFT: ['PLANNING', 'REJECTED'],
  PLANNING: ['READY', 'REJECTED'],
  READY: ['INPROGRESS', 'REJECTED'],
  INPROGRESS: ['NEEDSREVIEW', 'FAILED', 'REJECTED'],
  NEEDSREVIEW: ['INPROGRESS', 'DONE', 'REJECTED'],
  DONE: [],
  FAILED: [],
  REJECTED: [],
}

const AGENT_TASK_TRANSITIONS: Record<AgentTaskStatus, AgentTaskStatus[]> = {
  READY: ['INPROGRESS', 'REJECTED'],
  INPROGRESS: ['DONE', 'FAILED', 'NEEDSREVIEW', 'REJECTED'],
  NEEDSREVIEW: ['INPROGRESS', 'DONE', 'REJECTED'],
  DONE: [],
  FAILED: [],
  REJECTED: [],
}

export function isValidDeliverableTransition(from: DeliverableStatus, to: DeliverableStatus): boolean {
  const allowed = DELIVERABLE_TRANSITIONS[from]
  return allowed?.includes(to) ?? false
}

export function isValidAgentTaskTransition(from: AgentTaskStatus, to: AgentTaskStatus): boolean {
  const allowed = AGENT_TASK_TRANSITIONS[from]
  return allowed?.includes(to) ?? false
}
