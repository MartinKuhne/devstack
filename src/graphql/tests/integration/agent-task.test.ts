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

describe('AgentTask CRUD', () => {
  it('creates an agent task', async () => {
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

    const createDeliverableResult = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Test Deliverable"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) {
            id
          }
        }
      `,
    })
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    const result = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Test Task"
            description: "A test task"
            complexityRating: 5
          }) {
            id
            title
            status
            description
            complexityRating
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createAgentTask.title).toBe('Test Task')
    expect(result.body.singleResult.data.createAgentTask.status).toBe('READY')
    expect(result.body.singleResult.data.createAgentTask.complexityRating).toBe(5)
  })

  it('gets an agent task by id', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Find Me"
            description: "Find this task"
            complexityRating: 3
          }) {
            id
          }
        }
      `,
    })
    const taskId = createResult.body.singleResult.data.createAgentTask.id

    const result = await executeOperation({
      query: `
        query {
          agentTask(id: "${taskId}") {
            id
            title
            status
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.agentTask.title).toBe('Find Me')
  })

  it('returns null for non-existent agent task', async () => {
    const result = await executeOperation({
      query: `
        query {
          agentTask(id: "00000000-0000-0000-0000-000000000000") {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.agentTask).toBeNull()
  })

  it('updates an agent task', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Original"
            description: "Original"
          }) {
            id
          }
        }
      `,
    })
    const taskId = createResult.body.singleResult.data.createAgentTask.id

    const result = await executeOperation({
      query: `
        mutation {
          updateAgentTask(id: "${taskId}", input: { id: "${taskId}", title: "Updated", result: "Task completed" }) {
            id
            title
            result
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateAgentTask.title).toBe('Updated')
    expect(result.body.singleResult.data.updateAgentTask.result).toBe('Task completed')
  })

  it('returns null when updating non-existent agent task', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          updateAgentTask(id: "00000000-0000-0000-0000-000000000000", input: { id: "00000000-0000-0000-0000-000000000000", title: "Nope" }) {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateAgentTask).toBeNull()
  })

  it('updates agent task status', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Test"
            description: "Test"
          }) {
            id
          }
        }
      `,
    })
    const taskId = createResult.body.singleResult.data.createAgentTask.id

    const result = await executeOperation({
      query: `
        mutation {
          updateAgentTaskStatus(id: "${taskId}", targetStatus: INPROGRESS)
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateAgentTaskStatus).toBe('INPROGRESS')
  })

  it('deletes an agent task', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    const createResult = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Delete Me"
            description: "Delete this"
          }) {
            id
          }
        }
      `,
    })
    const taskId = createResult.body.singleResult.data.createAgentTask.id

    const result = await executeOperation({
      query: `
        mutation {
          deleteAgentTask(id: "${taskId}")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteAgentTask).toBe(true)
  })

  it('returns false when deleting non-existent agent task', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          deleteAgentTask(id: "00000000-0000-0000-0000-000000000000")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteAgentTask).toBe(false)
  })
})

describe('AgentTask Filtering', () => {
  it('filters agent tasks by deliverableId', async () => {
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

    const createDeliverable1Result = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Deliverable1"
            type: FEATURE
            description: "Test"
            initialStatus: DRAFT
          }) {
            id
          }
        }
      `,
    })
    const deliverableId1 = createDeliverable1Result.body.singleResult.data.createDeliverable.id

    const createDeliverable2Result = await executeOperation({
      query: `
        mutation {
          createDeliverable(input: {
            projectId: "${projectId}"
            title: "Deliverable2"
            type: DEFECT
            description: "Test"
            initialStatus: DRAFT
          }) {
            id
          }
        }
      `,
    })
    const deliverableId2 = createDeliverable2Result.body.singleResult.data.createDeliverable.id

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId1}"
            projectId: "${projectId}"
            title: "Task1"
            description: "Test"
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId2}"
            projectId: "${projectId}"
            title: "Task2"
            description: "Test"
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          agentTasks(deliverableId: "${deliverableId1}") {
            nodes {
              title
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.agentTasks.totalCount).toBe(1)
    expect(result.body.singleResult.data.agentTasks.nodes[0].title).toBe('Task1')
  })

  it('filters agent tasks by status', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Ready Task"
            description: "Test"
          }) { id }
        }
      `,
    })  // This task remains READY

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "InProgress Task"
            description: "Test"
          }) { id status }
        }
      `,
    })

    // Update status after creation
    const inProgressTask = await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "InProgress Task 2"
            description: "Test 2"
          }) {
            id status
          }
        }
      `,
    })
    const taskId = inProgressTask.body.singleResult.data.createAgentTask.id
    await executeOperation({
      query: `
        mutation {
          updateAgentTaskStatus(id: "${taskId}", targetStatus: INPROGRESS)
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          agentTasks(filter: { status: INPROGRESS }) {
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
    expect(result.body.singleResult.data.agentTasks.totalCount).toBe(1)
    expect(result.body.singleResult.data.agentTasks.nodes[0].status).toBe('INPROGRESS')
  })
})

describe('AgentTask Sorting', () => {
  it('sorts agent tasks by title', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Zebra"
            description: "Test"
          }) { id }
        }
      `,
    })

    await executeOperation({
      query: `
        mutation {
          createAgentTask(input: {
            deliverableId: "${deliverableId}"
            projectId: "${projectId}"
            title: "Alpha"
            description: "Test"
          }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          agentTasks(sort: { title: "asc" }) {
            nodes {
              title
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.agentTasks.nodes.map((n: any) => n.title)).toEqual(['Alpha', 'Zebra'])
  })
})

describe('AgentTask Pagination', () => {
  it('paginates agent tasks with first', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createAgentTask(input: {
              deliverableId: "${deliverableId}"
              projectId: "${projectId}"
              title: "Task ${i}"
              description: "Test"
            }) { id }
          }
        `,
      })
    }

    const result = await executeOperation({
      query: `
        query {
          agentTasks(first: 2) {
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
    expect(result.body.singleResult.data.agentTasks.totalCount).toBe(5)
    expect(result.body.singleResult.data.agentTasks.nodes.length).toBe(2)
    expect(result.body.singleResult.data.agentTasks.pageInfo.hasNextPage).toBe(true)
    expect(result.body.singleResult.data.agentTasks.pageInfo.hasPreviousPage).toBe(false)
  })

  it('paginates agent tasks with after cursor', async () => {
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

    const createDeliverableResult = await executeOperation({
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
    const deliverableId = createDeliverableResult.body.singleResult.data.createDeliverable.id

    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createAgentTask(input: {
              deliverableId: "${deliverableId}"
              projectId: "${projectId}"
              title: "Task ${i}"
              description: "Test"
            }) { id }
          }
        `,
      })
    }

    const firstPage = await executeOperation({
      query: `
        query {
          agentTasks(first: 2) {
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

    const endCursor = firstPage.body.singleResult.data.agentTasks.pageInfo.endCursor

    const secondPage = await executeOperation({
      query: `
        query {
          agentTasks(first: 2, after: "${endCursor}") {
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
    expect(secondPage.body.singleResult.data.agentTasks.nodes.length).toBe(2)
    expect(secondPage.body.singleResult.data.agentTasks.pageInfo.hasNextPage).toBe(true)
    expect(secondPage.body.singleResult.data.agentTasks.pageInfo.hasPreviousPage).toBe(true)
  })

  it('returns empty agent tasks when no data', async () => {
    const result = await executeOperation({
      query: `
        query {
          agentTasks {
            nodes {
              title
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.agentTasks.totalCount).toBe(0)
    expect(result.body.singleResult.data.agentTasks.nodes.length).toBe(0)
  })
})
