
import * as projectService from "../services/project.service.js"
import * as deliverableService from "../services/deliverable.service.js"
import * as agentTaskService from "../services/agent-task.service.js"
import * as llmService from "../services/llm.service.js"
import { prisma } from "../config/database.js"

const queryResolvers = {
  project: async (_: unknown, { id }: { id: string }) => {
    return projectService.getById(id)
  },

  projects: async (
    _: unknown,
    { first, after, filter, sort }: { first?: number; after?: string; filter?: Record<string, unknown>; sort?: Record<string, string> },
  ) => {
    return projectService.getAll({ first, after, filter, sort })
  },

  deliverable: async (_: unknown, { id }: { id: string }) => {
    return deliverableService.getById(id)
  },

  deliverables: async (
    _: unknown,
    { projectId, first, after, filter, sort }: { projectId?: string; first?: number; after?: string; filter?: Record<string, unknown>; sort?: Record<string, string> },
  ) => {
    return deliverableService.getAll({ projectId, first, after, filter, sort })
  },

  deliverablesCount: async (_: unknown, { projectId, statusFilter, typeFilter }: { projectId?: string; statusFilter?: string[]; typeFilter?: string[] }) => {
    const where: Record<string, unknown> = {}
    if (projectId) where.projectId = projectId
    if (statusFilter?.length) where.status = { in: statusFilter }
    if (typeFilter?.length) where.type = { in: typeFilter }
    return prisma.deliverable.count({ where })
  },

  agentTask: async (_: unknown, { id }: { id: string }) => {
    return agentTaskService.getById(id)
  },

  agentTasks: async (
    _: unknown,
    { deliverableId, first, after, filter, sort }: { deliverableId?: string; first?: number; after?: string; filter?: Record<string, unknown>; sort?: Record<string, string> },
  ) => {
    return agentTaskService.getAll({ deliverableId, first, after, filter, sort })
  },

  largeLanguageModel: async (_: unknown, { id }: { id: string }) => {
    return llmService.getById(id)
  },

  largeLanguageModels: async (
    _: unknown,
    { first, after, filter, sort }: { first?: number; after?: string; filter?: Record<string, unknown>; sort?: Record<string, string> },
  ) => {
    return llmService.getAll({ first, after, filter, sort })
  },
}

export { queryResolvers }
