import { z } from 'zod'
import * as projectService from "../services/project.service.js"
import * as deliverableService from "../services/deliverable.service.js"
import * as agentTaskService from "../services/agent-task.service.js"
import * as llmService from "../services/llm.service.js"
import { CreateProjectInputSchema, UpdateProjectInputSchema } from "../validations/project.schema.js"
import {
  CreateDeliverableInputSchema,
  UpdateDeliverableInputSchema,
} from "../validations/deliverable.schema.js"
import { CreateAgentTaskInputSchema, UpdateAgentTaskInputSchema } from "../validations/agent-task.schema.js"
import {
  CreateLargeLanguageModelInputSchema,
  UpdateLargeLanguageModelInputSchema,
} from "../validations/llm.schema.js"

const mutationResolvers = {
  createProject: async (_: unknown, { input }: { input: z.infer<typeof CreateProjectInputSchema> }) => {
    const parsed = CreateProjectInputSchema.parse(input)
    return projectService.createProject(parsed)
  },

  updateProject: async (
    _: unknown,
    { id, input }: { id: string; input: z.infer<typeof UpdateProjectInputSchema> },
  ) => {
    const parsed = UpdateProjectInputSchema.parse(input)
    return projectService.update(id, parsed)
  },

  deleteProject: async (_: unknown, { id }: { id: string }) => {
    return projectService.deleteProject(id)
  },

  createDeliverable: async (
    _: unknown,
    { input }: { input: z.infer<typeof CreateDeliverableInputSchema> },
  ) => {
    const parsed = CreateDeliverableInputSchema.parse(input)
    return deliverableService.createDeliverable(parsed)
  },

  updateDeliverable: async (
    _: unknown,
    { id, input }: { id: string; input: z.infer<typeof UpdateDeliverableInputSchema> },
  ) => {
    const parsed = UpdateDeliverableInputSchema.parse(input)
    return deliverableService.update(id, parsed)
  },

  updateDeliverableStatus: async (
    _: unknown,
    { id, targetStatus, actor }: { id: string; targetStatus: string; actor?: string },
  ) => {
    return deliverableService.updateStatus(id, targetStatus as 'DRAFT' | 'PLANNING' | 'READY' | 'INPROGRESS' | 'CODECOMPLETE' | 'TESTING' | 'DONE' | 'FAILED' | 'REJECTED' | 'NEEDSREVIEW', actor)
  },

  deleteDeliverable: async (_: unknown, { id }: { id: string }) => {
    return deliverableService.deleteDeliverable(id)
  },

  checkAndMarkDeliverableDone: async (_: unknown, { deliverableId }: { deliverableId: string }) => {
    return deliverableService.checkAndMarkDone(deliverableId)
  },

  createAgentTask: async (
    _: unknown,
    { input }: { input: z.infer<typeof CreateAgentTaskInputSchema> },
  ) => {
    const parsed = CreateAgentTaskInputSchema.parse(input)
    return agentTaskService.createAgentTask(parsed)
  },

  updateAgentTask: async (
    _: unknown,
    { id, input }: { id: string; input: z.infer<typeof UpdateAgentTaskInputSchema> },
  ) => {
    const parsed = UpdateAgentTaskInputSchema.parse(input)
    return agentTaskService.update(id, parsed)
  },

  updateAgentTaskStatus: async (
    _: unknown,
    { id, targetStatus }: { id: string; targetStatus: string },
  ) => {
    return agentTaskService.updateStatus(id, targetStatus)
  },

  deleteAgentTask: async (_: unknown, { id }: { id: string }) => {
    return agentTaskService.deleteAgentTask(id)
  },

  createLargeLanguageModel: async (
    _: unknown,
    { input }: { input: z.infer<typeof CreateLargeLanguageModelInputSchema> },
  ) => {
    const parsed = CreateLargeLanguageModelInputSchema.parse(input)
    return llmService.createLlm(parsed)
  },

  updateLargeLanguageModel: async (
    _: unknown,
    { id, input }: { id: string; input: z.infer<typeof UpdateLargeLanguageModelInputSchema> },
  ) => {
    const parsed = UpdateLargeLanguageModelInputSchema.parse(input)
    return llmService.update(id, parsed)
  },

  deleteLargeLanguageModel: async (_: unknown, { id }: { id: string }) => {
    return llmService.deleteLlm(id)
  },

  cleanupTestData: async () => {
    try {
      await prisma.agentTask.deleteMany()
      await prisma.deliverable.deleteMany()
      await prisma.largeLanguageModel.deleteMany()
      await prisma.project.deleteMany()

      return { success: true, message: 'All test data cleaned up successfully' }
    } catch (error) {
      return { success: false, message: error instanceof Error ? error.message : 'Cleanup failed' }
    }
  },
}

import { prisma } from "../config/database.js"

export { mutationResolvers }
