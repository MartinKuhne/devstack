import { prisma } from "../config/database.js"
import { DeliverableStatus } from '@prisma/client'
import { buildWhereClause } from "../utils/filtering.js"
import { paginate, PaginatedResult } from "../utils/pagination.js"
import { isValidDeliverableTransition } from "./transition.service.js"

export async function createDeliverable(data: {
  projectId: string
  title: string
  type: 'FEATURE' | 'DEFECT' | 'MAINTENANCE'
  description: string
  initialStatus: DeliverableStatus
  acceptanceCriteria?: string
  executionPlan?: string
  securityImpact?: string
  performanceImpact?: string
  testPlan?: string
  deploymentPlan?: string
}): Promise<{
  id: string
  projectId: string
  type: string
  title: string
  status: string
  description: string | null
  acceptanceCriteria: string | null
  executionPlan: string | null
  agentFeedback: string | null
  securityImpact: string | null
  performanceImpact: string | null
  testPlan: string | null
  deploymentPlan: string | null
  blocking: string | null
}> {
  return prisma.deliverable.create({
    data: {
      projectId: data.projectId,
      title: data.title,
      type: data.type,
      status: data.initialStatus,
      description: data.description,
      acceptanceCriteria: data.acceptanceCriteria ?? null,
      executionPlan: data.executionPlan ?? null,
      securityImpact: data.securityImpact ?? null,
      performanceImpact: data.performanceImpact ?? null,
      testPlan: data.testPlan ?? null,
      deploymentPlan: data.deploymentPlan ?? null,
    },
  })
}

export async function getById(id: string) {
  return prisma.deliverable.findUnique({
    where: { id },
    include: { project: true },
  })
}

export async function getAll({
  projectId,
  first,
  after,
  filter,
  sort,
}: {
  projectId?: string
  first?: number
  after?: string
  filter?: Record<string, unknown>
  sort?: Record<string, string>
} = {}): Promise<PaginatedResult<unknown>> {
  const where: Record<string, unknown> = {}
  if (projectId) where.projectId = projectId
  if (filter) {
    Object.assign(where, buildWhereClause(filter))
  }

  const totalCount = await prisma.deliverable.count({ where })
  const allDeliverables = await prisma.deliverable.findMany({
    where,
    orderBy: sort || { id: 'desc' },
    include: { project: true },
  })

  return paginate(allDeliverables, totalCount, first, after)
}

export async function update(
  id: string,
  data: {
    title?: string
    description?: string
    acceptanceCriteria?: string
    agentFeedback?: string
    executionPlan?: string
    securityImpact?: string
    performanceImpact?: string
    testPlan?: string
    deploymentPlan?: string
    blocking?: string
  },
): Promise<unknown | null> {
  const existing = await prisma.deliverable.findUnique({ where: { id } })
  if (!existing) return null

  const updateData: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(data)) {
    if (value !== undefined) {
      updateData[key] = value
    }
  }

  return prisma.deliverable.update({
    where: { id },
    data: updateData,
  })
}

export async function updateStatus(
  id: string,
  targetStatus: DeliverableStatus,
  _actor?: string,
): Promise<DeliverableStatus | null> {
  // actor is reserved for audit trail logging
  const existing = await prisma.deliverable.findUnique({ where: { id } })
  if (!existing) return null

  const currentStatus = existing.status as DeliverableStatus
  if (!isValidDeliverableTransition(currentStatus, targetStatus)) {
    throw new Error(
      `Invalid transition from ${currentStatus} to ${targetStatus}`,
    )
  }

  await prisma.deliverable.update({
    where: { id },
    data: { status: targetStatus },
  })

  return targetStatus
}

export async function deleteDeliverable(id: string): Promise<boolean> {
  const existing = await prisma.deliverable.findUnique({ where: { id } })
  if (!existing) return false

  await prisma.deliverable.delete({ where: { id } })
  return true
}

export async function checkAndMarkDone(deliverableId: string): Promise<boolean> {
  const allTasks = await prisma.agentTask.findMany({
    where: { deliverableId },
  })

  if (allTasks.length === 0) return false

  const allDone = allTasks.every((task: any) => task.status === 'DONE')
  if (!allDone) return false

  await prisma.deliverable.update({
    where: { id: deliverableId },
    data: { status: 'DONE' },
  })

  return true
}
