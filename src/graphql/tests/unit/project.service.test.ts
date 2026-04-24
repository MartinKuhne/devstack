import { describe, it, expect, vi, beforeEach } from 'vitest'
import { z } from 'zod'
import { CreateProjectInputSchema, UpdateProjectInputSchema } from '../../src/validations/project.schema'

describe('Project Validation Schemas', () => {
  describe('CreateProjectInputSchema', () => {
    it('accepts valid input', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: 'Test Project',
        repository: 'https://github.com/test/project',
        description: 'A test project',
      })
      expect(result.success).toBe(true)
    })

    it('accepts input without description', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: 'Test Project',
        repository: 'https://github.com/test/project',
      })
      expect(result.success).toBe(true)
    })

    it('rejects missing name', () => {
      const result = CreateProjectInputSchema.safeParse({
        repository: 'https://github.com/test/project',
      })
      expect(result.success).toBe(false)
    })

    it('rejects empty name', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: '',
        repository: 'https://github.com/test/project',
      })
      expect(result.success).toBe(false)
    })

    it('rejects name exceeding max length', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: 'a'.repeat(201),
        repository: 'https://github.com/test/project',
      })
      expect(result.success).toBe(false)
    })

    it('rejects missing repository', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: 'Test Project',
      })
      expect(result.success).toBe(false)
    })

    it('rejects repository exceeding max length', () => {
      const result = CreateProjectInputSchema.safeParse({
        name: 'Test Project',
        repository: 'a'.repeat(501),
      })
      expect(result.success).toBe(false)
    })

    it('validates required fields throw on parse', () => {
      expect(() => CreateProjectInputSchema.parse({})).toThrow()
      expect(() => CreateProjectInputSchema.parse({ name: 'Test' })).toThrow()
    })
  })

  describe('UpdateProjectInputSchema', () => {
    it('accepts partial update', () => {
      const result = UpdateProjectInputSchema.safeParse({
        name: 'Updated Name',
      })
      expect(result.success).toBe(true)
    })

    it('accepts empty object', () => {
      const result = UpdateProjectInputSchema.safeParse({})
      expect(result.success).toBe(true)
    })

    it('accepts all fields', () => {
      const result = UpdateProjectInputSchema.safeParse({
        name: 'Updated',
        repository: 'https://github.com/test/updated',
        description: 'Updated description',
      })
      expect(result.success).toBe(true)
    })

    it('rejects name exceeding max length', () => {
      const result = UpdateProjectInputSchema.safeParse({
        name: 'a'.repeat(201),
      })
      expect(result.success).toBe(false)
    })
  })
})
