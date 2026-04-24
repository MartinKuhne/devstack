import { z } from 'zod'

export const CreateAgentTaskInputSchema = z.object({
  deliverableId: z.string().min(1),
  projectId: z.string().min(1),
  title: z.string().min(1).max(300),
  description: z.string().default(''),
  dependsOnAgentTaskId: z.string().min(1).optional().nullable(),
  complexityRating: z.number().min(1).max(10).default(5),
})

export const UpdateAgentTaskInputSchema = z.object({
  title: z.string().min(1).max(300).optional(),
  description: z.string().optional(),
  result: z.string().optional(),
  errors: z.string().optional(),
  commitHash: z.string().max(64).optional(),
  dependsOnAgentTaskId: z.string().min(1).optional().nullable(),
  complexityRating: z.number().min(1).max(10).optional(),
  promptTokens: z.number().optional(),
  completionTokens: z.number().optional(),
  executionDurationInSeconds: z.number().optional(),
  agent: z.string().max(100).optional(),
})

export const UpdateAgentTaskStatusInputSchema = z.object({
  targetStatus: z.enum(['READY', 'INPROGRESS', 'DONE', 'FAILED', 'REJECTED', 'NEEDSREVIEW']),
})

export type CreateAgentTaskInput = z.infer<typeof CreateAgentTaskInputSchema>
export type UpdateAgentTaskInput = z.infer<typeof UpdateAgentTaskInputSchema>
export type UpdateAgentTaskStatusInput = z.infer<typeof UpdateAgentTaskStatusInputSchema>
