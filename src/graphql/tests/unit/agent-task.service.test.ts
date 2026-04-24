import { describe, it, expect } from 'vitest'
import { CreateAgentTaskInputSchema, UpdateAgentTaskInputSchema } from '../../src/validations/agent-task.schema'

describe('AgentTask Validation Schemas', () => {
  describe('CreateAgentTaskInputSchema', () => {
    it('accepts valid input with defaults', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
      })
      expect(result.success).toBe(true)
    })

    it('accepts input with complexity rating', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
        complexityRating: 5,
      })
      expect(result.success).toBe(true)
    })

    it('accepts input with dependency', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
        dependsOnAgentTaskId: 't456',
      })
      expect(result.success).toBe(true)
    })

    it('rejects complexity rating below minimum', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
        complexityRating: 0,
      })
      expect(result.success).toBe(false)
    })

    it('rejects complexity rating above maximum', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
        complexityRating: 11,
      })
      expect(result.success).toBe(false)
    })

    it('rejects missing deliverableId', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        projectId: 'p123',
        title: 'Test Task',
        description: 'Test description',
      })
      expect(result.success).toBe(false)
    })

    it('rejects missing projectId', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        title: 'Test Task',
        description: 'Test description',
      })
      expect(result.success).toBe(false)
    })

    it('rejects missing title', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        description: 'Test description',
      })
      expect(result.success).toBe(false)
    })

    it('rejects title exceeding max length', () => {
      const result = CreateAgentTaskInputSchema.safeParse({
        deliverableId: 'd123',
        projectId: 'p123',
        title: 'a'.repeat(301),
        description: 'Test description',
      })
      expect(result.success).toBe(false)
    })
  })

  describe('UpdateAgentTaskInputSchema', () => {
    it('accepts partial update', () => {
      const result = UpdateAgentTaskInputSchema.safeParse({
        title: 'Updated Title',
      })
      expect(result.success).toBe(true)
    })

    it('accepts all fields', () => {
      const result = UpdateAgentTaskInputSchema.safeParse({
        title: 'Updated',
        description: 'Updated desc',
        result: 'Success',
        errors: '',
        commitHash: 'abc123',
        complexityRating: 7,
        promptTokens: 100,
        completionTokens: 200,
        executionDurationInSeconds: 30,
        agent: 'test-agent',
      })
      expect(result.success).toBe(true)
    })

    it('accepts empty object', () => {
      const result = UpdateAgentTaskInputSchema.safeParse({})
      expect(result.success).toBe(true)
    })

    it('rejects complexity below minimum', () => {
      const result = UpdateAgentTaskInputSchema.safeParse({
        complexityRating: 0,
      })
      expect(result.success).toBe(false)
    })

    it('rejects complexity above maximum', () => {
      const result = UpdateAgentTaskInputSchema.safeParse({
        complexityRating: 11,
      })
      expect(result.success).toBe(false)
    })
  })
})
