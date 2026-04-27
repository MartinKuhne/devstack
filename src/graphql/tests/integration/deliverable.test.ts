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

describe('Deliverable CRUD', () => {
  it('creates a deliverable', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    const result = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Test Deliverable"
            type: FEATURE
            description: "A test deliverable"
            initialStatus: DRAFT
          }) {
            id
            title
            type
            status
            description
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createDeliverable.title).toBe('Test Deliverable')
    expect(result.body.singleResult.data.createDeliverable.type).toBe('FEATURE')
    expect(result.body.singleResult.data.createDeliverable.status).toBe('DRAFT')
  })

  it('gets a deliverable by id', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Find Me"
            type: DEFECT
            description: "Find this"
            initialStatus: PLANNING
          }) {
            id
          }
        }
      `,
    })
    const deliverableId = createResult.body.singleResult.data.createDeliverable.id

    const result = await executeOperation({
      query: `
        query {
          deliverable(id: "${deliverableId}") {
            id
            title
            type
            status
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverable.title).toBe('Find Me')
  })

  it('returns null for non-existent deliverable', async () => {
    const result = await executeOperation({
      query: `
        query {
          deliverable(id: "00000000-0000-0000-0000-000000000000") {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverable).toBeNull()
  })

  it('updates a deliverable', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Original"
            type: MAINTENANCE
            description: "Original desc"
            initialStatus: READY
          }) {
            id
          }
        }
      `,
    })
    const deliverableId = createResult.body.singleResult.data.createDeliverable.id

    const result = await executeOperation({
      query: `
        mutation {
          updateDeliverable(id: "${deliverableId}", input: { id: "${deliverableId}", title: "Updated", executionPlan: "Step 1, Step 2" }) {
            id
            title
            executionPlan
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateDeliverable.title).toBe('Updated')
    expect(result.body.singleResult.data.updateDeliverable.executionPlan).toBe('Step 1, Step 2')
  })

  it('returns null when updating non-existent deliverable', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          updateDeliverable(id: "00000000-0000-0000-0000-000000000000", input: { id: "00000000-0000-0000-0000-000000000000", title: "Nope" }) {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateDeliverable).toBeNull()
  })

  it('updates deliverable status', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Test"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) {
            id
          }
        }
      `,
    })
    const deliverableId = createResult.body.singleResult.data.createDeliverable.id

    const result = await executeOperation({
      query: `
        mutation {
          updateDeliverableStatus(id: "${deliverableId}", targetStatus: PLANNING)
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateDeliverableStatus).toBe('PLANNING')
  })

  it('deletes a deliverable', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Delete Me"
            type: FEATURE
            description: "Delete this"
            initialStatus: DRAFT
          }) {
            id
          }
        }
      `,
    })
    const deliverableId = createResult.body.singleResult.data.createDeliverable.id

    const result = await executeOperation({
      query: `
        mutation {
          deleteDeliverable(id: "${deliverableId}")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteDeliverable).toBe(true)
  })

  it('returns false when deleting non-existent deliverable', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          deleteDeliverable(id: "00000000-0000-0000-0000-000000000000")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteDeliverable).toBe(false)
  })
})

describe('Deliverable Filtering', () => {
  it('filters deliverables by projectId', async () => {
    const createProject1Result = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Project1", repository: "https://github.com/test/repo1" }) {
            id
          }
        }
      `,
    })
    const projectId1 = createProject1Result.body.singleResult.data.createProject.id

    const createProject2Result = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Project2", repository: "https://github.com/test/repo2" }) {
            id
          }
        }
      `,
    })
    const projectId2 = createProject2Result.body.singleResult.data.createProject.id

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId1}"
            title: "D1"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId2}"
            title: "D2"
            type: DEFECT
            description: "Test"
            initialStatus: PLANNING
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          deliverables(projectId: "${projectId1}") {
            nodes {
              title
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverables.totalCount).toBe(1)
    expect(result.body.singleResult.data.deliverables.nodes[0].title).toBe('D1')
  })

  it('filters deliverables by status', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Draft"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Planning"
            type: DEFECT
            description: "Test"
            initialStatus: PLANNING
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          deliverables(filter: { status: DRAFT }) {
            nodes {
              title
              status
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverables.totalCount).toBe(1)
    expect(result.body.singleResult.data.deliverables.nodes[0].status).toBe('DRAFT')
  })

  it('filters deliverables by type', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Feature"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Defect"
            type: DEFECT
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          deliverables(filter: { type: DEFECT }) {
            nodes {
              title
              type
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverables.totalCount).toBe(1)
    expect(result.body.singleResult.data.deliverables.nodes[0].type).toBe('DEFECT')
  })

  it('counts deliverables with filters', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "F1"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "F2"
            type: FEATURE
            description: "Test"
            initialStatus: PLANNING
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "D1"
            type: DEFECT
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          deliverablesCount(projectId: "${projectId}", statusFilter: [DRAFT], typeFilter: [FEATURE])
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverablesCount).toBe(1)
  })
})

describe('Deliverable Sorting', () => {
  it('sorts deliverables by title', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Charlie"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Alpha"
            type: DEFECT
            description: "Test"
            initialStatus: DRAFT
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          deliverables(sort: { title: "asc" }) {
            nodes {
              title
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverables.nodes.map((n: any) => n.title)).toEqual(['Alpha', 'Charlie'])
  })
})

describe('Deliverable Pagination', () => {
  it('paginates deliverables with first', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createDeliverable(input: {
              projectId: "${projectId}"
              title: "Deliverable ${i}"
              type: FEATURE
              description: "Test"
              initialStatus: DRAFT
            }) { id }
          }
        `,
      })
    }

    const result = await executeOperation({
      query: `
        query {
          deliverables(first: 2) {
            nodes {
              title
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
    expect(result.body.singleResult.data.deliverables.totalCount).toBe(5)
    expect(result.body.singleResult.data.deliverables.nodes.length).toBe(2)
    expect(result.body.singleResult.data.deliverables.pageInfo.hasNextPage).toBe(true)
  })

  it('paginates deliverables with after cursor', async () => {
    const createProjectResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createProjectResult.body.singleResult.data.createProject.id

    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createDeliverable(input: {
              projectId: "${projectId}"
              title: "Deliverable ${i}"
              type: FEATURE
              description: "Test"
              initialStatus: DRAFT
            }) { id }
          }
        `,
      })
    }

    const firstPage = await executeOperation({
      query: `
        query {
          deliverables(first: 2) {
            nodes {
              title
            }
            pageInfo {
              endCursor
              hasNextPage
            }
          }
        }
      `,
    })

    const endCursor = firstPage.body.singleResult.data.deliverables.pageInfo.endCursor

    const secondPage = await executeOperation({
      query: `
        query {
          deliverables(first: 2, after: "${endCursor}") {
            nodes {
              title
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
    expect(secondPage.body.singleResult.data.deliverables.nodes.length).toBe(2)
    expect(secondPage.body.singleResult.data.deliverables.pageInfo.hasNextPage).toBe(true)
    expect(secondPage.body.singleResult.data.deliverables.pageInfo.hasPreviousPage).toBe(true)
  })

  it('returns empty deliverables when no data', async () => {
    const result = await executeOperation({
      query: `
        query {
          deliverables {
            nodes {
              title
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deliverables.totalCount).toBe(0)
    expect(result.body.singleResult.data.deliverables.nodes.length).toBe(0)
  })
})
