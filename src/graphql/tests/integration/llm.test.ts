import { beforeAll, beforeEach, afterAll, describe, it, expect, vi } from 'vitest'
import { ApolloServer } from '@apollo/server'
import { InMemoryStore, createMockPrisma } from './helpers/mock-prisma.js'

var _store: InMemoryStore

vi.mock('../../src/config/database.js', () => {
  _store = new InMemoryStore()
  const mockPrisma = createMockPrisma(_store)
  return {
    prisma: mockPrisma,
    connectDatabase: async () => {},
    disconnectDatabase: async () => {},
  }
})

import { createApolloServer } from '../../src/server.js'

let server: ApolloServer
let executeOperation: any

beforeAll(async () => {
  server = await createApolloServer()
  executeOperation = server.executeOperation.bind(server)
})

beforeEach(() => {
  _store!.clear()
})

afterAll(async () => {
  await server.stop()
})

describe('LargeLanguageModel CRUD', () => {
  it('creates a large language model', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-test-key-123"
            maxComplexity: 8
            maxConcurrency: 3
          }) {
            id
            url
            model
            apiKey
            cost
            maxComplexity
            maxConcurrency
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createLargeLanguageModel.model).toBe('gpt-4')
    expect(result.body.singleResult.data.createLargeLanguageModel.url).toBe('https://api.openai.com')
    expect(result.body.singleResult.data.createLargeLanguageModel.maxComplexity).toBe(8)
    expect(result.body.singleResult.data.createLargeLanguageModel.maxConcurrency).toBe(3)
  })

  it('creates a large language model with optional fields', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.example.com"
            model: "llama-3"
            modelAlias: "Llama"
            apiKey: "sk-test-key-456"
            cost: 5
            maxComplexity: 10
            maxConcurrency: 5
          }) {
            id
            modelAlias
            cost
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createLargeLanguageModel.modelAlias).toBe('Llama')
    expect(result.body.singleResult.data.createLargeLanguageModel.cost).toBe(5)
  })

  it('gets a large language model by id', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-test-key-123"
          }) {
            id
          }
        }
      `,
    })
    const llmId = createResult.body.singleResult.data.createLargeLanguageModel.id

    const result = await executeOperation({
      query: `
        query {
          largeLanguageModel(id: "${llmId}") {
            id
            model
            url
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModel.model).toBe('gpt-4')
  })

  it('returns null for non-existent large language model', async () => {
    const result = await executeOperation({
      query: `
        query {
          largeLanguageModel(id: "00000000-0000-0000-0000-000000000000") {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModel).toBeNull()
  })

  it('updates a large language model', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-test-key-123"
          }) {
            id
          }
        }
      `,
    })
    const llmId = createResult.body.singleResult.data.createLargeLanguageModel.id

    const result = await executeOperation({
      query: `
        mutation {
          updateLargeLanguageModel(id: "${llmId}", input: { id: "${llmId}", model: "gpt-4-turbo", maxComplexity: 9 }) {
            id
            model
            maxComplexity
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateLargeLanguageModel.model).toBe('gpt-4-turbo')
    expect(result.body.singleResult.data.updateLargeLanguageModel.maxComplexity).toBe(9)
  })

  it('returns null when updating non-existent large language model', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          updateLargeLanguageModel(id: "00000000-0000-0000-0000-000000000000", input: { id: "00000000-0000-0000-0000-000000000000", model: "Nope" }) {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateLargeLanguageModel).toBeNull()
  })

  it('deletes a large language model', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-test-key-123"
          }) {
            id
          }
        }
      `,
    })
    const llmId = createResult.body.singleResult.data.createLargeLanguageModel.id

    const result = await executeOperation({
      query: `
        mutation {
          deleteLargeLanguageModel(id: "${llmId}")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteLargeLanguageModel).toBe(true)
  })

  it('returns false when deleting non-existent large language model', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          deleteLargeLanguageModel(id: "00000000-0000-0000-0000-000000000000")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteLargeLanguageModel).toBe(false)
  })
})

describe('LargeLanguageModel Filtering', () => {
  it('filters large language models by model', async () => {
    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-key-1"
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.anthropic.com"
            model: "claude-3"
            apiKey: "sk-key-2"
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          largeLanguageModels(filter: { model: "gpt-4" }) {
            nodes {
              model
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModels.totalCount).toBe(1)
    expect(result.body.singleResult.data.largeLanguageModels.nodes[0].model).toBe('gpt-4')
  })

  it('filters large language models by url', async () => {
    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.openai.com"
            model: "gpt-4"
            apiKey: "sk-key-1"
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.anthropic.com"
            model: "claude-3"
            apiKey: "sk-key-2"
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          largeLanguageModels(filter: { url: "https://api.openai.com" }) {
            nodes {
              url
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModels.totalCount).toBe(1)
    expect(result.body.singleResult.data.largeLanguageModels.nodes[0].url).toBe('https://api.openai.com')
  })
})

describe('LargeLanguageModel Sorting', () => {
  it('sorts large language models by model', async () => {
    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.example.com"
            model: "Zebra"
            apiKey: "sk-key-1"
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createLargeLanguageModel(input: {
            url: "https://api.example.com"
            model: "Alpha"
            apiKey: "sk-key-2"
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          largeLanguageModels(sort: { model: "asc" }) {
            nodes {
              model
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModels.nodes.map((n: any) => n.model)).toEqual(['Alpha', 'Zebra'])
  })
})

describe('LargeLanguageModel Pagination', () => {
  it('paginates large language models with first', async () => {
    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createLargeLanguageModel(input: {
              url: "https://api.example.com"
              model: "Model ${i}"
              apiKey: "sk-key-${i}"
            }) { id }
          }
        `,
      })
    }

    const result = await executeOperation({
      query: `
        query {
          largeLanguageModels(first: 2) {
            nodes {
              model
            }
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModels.totalCount).toBe(5)
    expect(result.body.singleResult.data.largeLanguageModels.nodes.length).toBe(2)
    expect(result.body.singleResult.data.largeLanguageModels.pageInfo.hasNextPage).toBe(true)
    expect(result.body.singleResult.data.largeLanguageModels.pageInfo.hasPreviousPage).toBe(false)
  })

  it('paginates large language models with after cursor', async () => {
    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createLargeLanguageModel(input: {
              url: "https://api.example.com"
              model: "Model ${i}"
              apiKey: "sk-key-${i}"
            }) { id }
          }
        `,
      })
    }

    const firstPage = await executeOperation({
      query: `
        query {
          largeLanguageModels(first: 2) {
            nodes {
              model
            }
            pageInfo {
              endCursor
              hasNextPage
            }
          }
        }
      `,
    })

    const endCursor = firstPage.body.singleResult.data.largeLanguageModels.pageInfo.endCursor

    const secondPage = await executeOperation({
      query: `
        query {
          largeLanguageModels(first: 2, after: "${endCursor}") {
            nodes {
              model
            }
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
          }
        }
      `,
    })

    expect(secondPage.body.singleResult.errors).toBeUndefined()
    expect(secondPage.body.singleResult.data.largeLanguageModels.nodes.length).toBe(2)
    expect(secondPage.body.singleResult.data.largeLanguageModels.pageInfo.hasNextPage).toBe(true)
    expect(secondPage.body.singleResult.data.largeLanguageModels.pageInfo.hasPreviousPage).toBe(true)
  })

  it('returns empty large language models when no data', async () => {
    const result = await executeOperation({
      query: `
        query {
          largeLanguageModels {
            nodes {
              model
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.largeLanguageModels.totalCount).toBe(0)
    expect(result.body.singleResult.data.largeLanguageModels.nodes.length).toBe(0)
  })
})
