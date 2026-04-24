import { z } from 'zod'

export const CreateDeliverableInputSchema = z.object({
  projectId: z.string().uuid(),
  title: z.string().min(1).max(200),
  type: z.enum(['FEATURE', 'DEFECT', 'MAINTENANCE']),
  description: z.string(),
  initialStatus: z.enum(['DRAFT', 'PLANNING', 'READY', 'INPROGRESS', 'DONE', 'FAILED', 'REJECTED', 'NEEDSREVIEW']),
  acceptanceCriteria: z.string().optional(),
  executionPlan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
})

export const UpdateDeliverableInputSchema = z.object({
  title: z.string().min(1).max(200).optional(),
  description: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  agentFeedback: z.string().optional(),
  executionPlan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
  blocking: z.string().optional(),
})

export const UpdateDeliverableStatusInputSchema = z.object({
  targetStatus: z.enum(['DRAFT', 'PLANNING', 'READY', 'INPROGRESS', 'DONE', 'FAILED', 'REJECTED', 'NEEDSREVIEW']),
  actor: z.string().optional(),
})

export type CreateDeliverableInput = z.infer<typeof CreateDeliverableInputSchema>
export type UpdateDeliverableInput = z.infer<typeof UpdateDeliverableInputSchema>
export type UpdateDeliverableStatusInput = z.infer<typeof UpdateDeliverableStatusInputSchema>
