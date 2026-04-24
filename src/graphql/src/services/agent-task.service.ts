import { prisma } from "../config/database.js"
import { buildWhereClause } from "../utils/filtering.js"
import { paginate, PaginatedResult } from "../utils/pagination.js"
import { isValidAgentTaskTransition } from "./transition.service.js"

export async function createAgentTask(data: {
  deliverableId: string
  projectId: string
  title: string
  description: string
  dependsOnAgentTaskId?: string | null
  complexityRating?: number
}): Promise<{
  id: string
  projectId: string
  deliverableId: string
  title: string
  status: string
  description: string
  result: string | null
  errors: string | null
  commitHash: string | null
  complexityRating: number
  dependsOnAgentTaskId: string | null
  promptTokens: number | null
  completionTokens: number | null
  executionDurationInSeconds: number | null
  agent: string | null
}> {
  if (data.dependsOnAgentTaskId) {
    const dependsOn = await prisma.agentTask.findUnique({
      where: { id: data.dependsOnAgentTaskId },
    })
    if (!dependsOn) {
      throw new Error(`AgentTask with id ${data.dependsOnAgentTaskId} not found`)
    }
  }

  return prisma.agentTask.create({
    data: {
      deliverableId: data.deliverableId,
      projectId: data.projectId,
      title: data.title,
      description: data.description,
      dependsOnAgentTaskId: data.dependsOnAgentTaskId,
      complexityRating: data.complexityRating ?? 1,
    },
  })
}

export async function getById(id: string) {
  return prisma.agentTask.findUnique({
    where: { id },
  })
}

export async function getAll({
  deliverableId,
  first,
  after,
  filter,
  sort,
}: {
  deliverableId?: string
  first?: number
  after?: string
  filter?: Record<string, unknown>
  sort?: Record<string, string>
} = {}): Promise<PaginatedResult<unknown>> {
  const where: Record<string, unknown> = {}
  if (deliverableId) where.deliverableId = deliverableId
  if (filter) {
    Object.assign(where, buildWhereClause(filter))
  }

  const totalCount = await prisma.agentTask.count({ where })
  const allTasks = await prisma.agentTask.findMany({
    where,
    orderBy: sort || { id: 'desc' },
  })

  return paginate(allTasks, totalCount, first, after)
}

export async function update(
  id: string,
  data: {
    title?: string
    description?: string
    result?: string
    errors?: string
    commitHash?: string
    dependsOnAgentTaskId?: string | null
    complexityRating?: number
    promptTokens?: number
    completionTokens?: number
    executionDurationInSeconds?: number
    agent?: string
  },
): Promise<unknown | null> {
  if (data.dependsOnAgentTaskId) {
    const dependsOn = await prisma.agentTask.findUnique({
      where: { id: data.dependsOnAgentTaskId },
    })
    if (!dependsOn) {
      throw new Error(`AgentTask with id ${data.dependsOnAgentTaskId} not found`)
    }
  }

  const existing = await prisma.agentTask.findUnique({ where: { id } })
  if (!existing) return null

  const updateData: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(data)) {
    if (value !== undefined) {
      updateData[key] = value
    }
  }

  return prisma.agentTask.update({
    where: { id },
    data: updateData,
  })
}

export async function updateStatus(id: string, targetStatus: string): Promise<string | null> {
  const existing = await prisma.agentTask.findUnique({ where: { id } })
  if (!existing) return null

  const currentStatus = existing.status as 'READY' | 'INPROGRESS' | 'DONE' | 'FAILED' | 'REJECTED' | 'NEEDSREVIEW'
  if (!isValidAgentTaskTransition(currentStatus, targetStatus as typeof currentStatus)) {
    throw new Error(`Invalid transition from ${currentStatus} to ${targetStatus}`)
  }

  await prisma.agentTask.update({
    where: { id },
    data: { status: targetStatus as 'READY' | 'INPROGRESS' | 'DONE' | 'FAILED' | 'REJECTED' | 'NEEDSREVIEW' },
  })

  return targetStatus
}

export async function deleteAgentTask(id: string): Promise<boolean> {
  const existing = await prisma.agentTask.findUnique({ where: { id } })
  if (!existing) return false

  await prisma.agentTask.delete({ where: { id } })
  return true
}
