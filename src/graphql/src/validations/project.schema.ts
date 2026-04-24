import { z } from 'zod'

export const CreateProjectInputSchema = z.object({
  name: z.string().min(1).max(200),
  repository: z.string().min(1).max(500),
  description: z.string().max(10000).optional(),
})

export const UpdateProjectInputSchema = z.object({
  name: z.string().min(1).max(200).optional(),
  repository: z.string().min(1).max(500).optional(),
  description: z.string().max(10000).optional(),
})

export type CreateProjectInput = z.infer<typeof CreateProjectInputSchema>
export type UpdateProjectInput = z.infer<typeof UpdateProjectInputSchema>
