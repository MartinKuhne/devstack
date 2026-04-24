import { z } from 'zod'

export const CreateLargeLanguageModelInputSchema = z.object({
  url: z.string().min(1).max(500),
  model: z.string().min(1).max(200),
  modelAlias: z.string().max(100).optional(),
  apiKey: z.string().min(1).max(1000),
  maxComplexity: z.number().min(1).max(10).default(10),
  maxConcurrency: z.number().min(1).default(1),
})

export const UpdateLargeLanguageModelInputSchema = z.object({
  url: z.string().min(1).max(500).optional(),
  model: z.string().min(1).max(200).optional(),
  modelAlias: z.string().max(100).optional(),
  apiKey: z.string().min(1).max(1000).optional(),
  maxComplexity: z.number().min(1).max(10).optional(),
  maxConcurrency: z.number().min(1).optional(),
})

export type CreateLargeLanguageModelInput = z.infer<typeof CreateLargeLanguageModelInputSchema>
export type UpdateLargeLanguageModelInput = z.infer<typeof UpdateLargeLanguageModelInputSchema>
