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

describe('Project CRUD', () => {
  it('creates a project', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test Project", repository: "https://github.com/test/repo" }) {
            id
            name
            repository
            description
          }
        }
      `,
    })
    console.log('RESULT:', JSON.stringify(result, null, 2))

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createProject.name).toBe('Test Project')
    expect(result.body.singleResult.data.createProject.repository).toBe('https://github.com/test/repo')
    expect(result.body.singleResult.data.createProject.description).toBeNull()
  })

  it('creates a project with description', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Test", repository: "https://github.com/test/repo", description: "A test project" }) {
            id
            name
            description
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.createProject.description).toBe('A test project')
  })

  it('gets a project by id', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Find Me", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createResult.body.singleResult.data.createProject.id

    const result = await executeOperation({
      query: `
        query {
          project(id: "${projectId}") {
            id
            name
            repository
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.project.name).toBe('Find Me')
  })

  it('returns null for non-existent project', async () => {
    const result = await executeOperation({
      query: `
        query {
          project(id: "00000000-0000-0000-0000-000000000000") {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.project).toBeNull()
  })

  it('updates a project', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Original", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createResult.body.singleResult.data.createProject.id

    const result = await executeOperation({
      query: `
        mutation {
          updateProject(id: "${projectId}", input: { id: "${projectId}", name: "Updated" }) {
            id
            name
            repository
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateProject.name).toBe('Updated')
    expect(result.body.singleResult.data.updateProject.repository).toBe('https://github.com/test/repo')
  })

  it('returns null when updating non-existent project', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          updateProject(id: "00000000-0000-0000-0000-000000000000", input: { id: "00000000-0000-0000-0000-000000000000", name: "Nope" }) {
            id
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.updateProject).toBeNull()
  })

  it('deletes a project', async () => {
    const createResult = await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Delete Me", repository: "https://github.com/test/repo" }) {
            id
          }
        }
      `,
    })
    const projectId = createResult.body.singleResult.data.createProject.id

    const result = await executeOperation({
      query: `
        mutation {
          deleteProject(id: "${projectId}")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteProject).toBe(true)

    const getResult = await executeOperation({
      query: `
        query {
          project(id: "${projectId}") {
            id
          }
        }
      `,
    })
    expect(getResult.body.singleResult.data.project).toBeNull()
  })

  it('returns false when deleting non-existent project', async () => {
    const result = await executeOperation({
      query: `
        mutation {
          deleteProject(id: "00000000-0000-0000-0000-000000000000")
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.deleteProject).toBe(false)
  })
})

describe('Project Filtering', () => {
  it('filters projects by name', async () => {
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Alpha", repository: "https://github.com/a/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Beta", repository: "https://github.com/b/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Alpha Two", repository: "https://github.com/c/repo" }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          projects(filter: { name: "Alpha" }) {
            nodes {
              name
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.totalCount).toBe(1)
    expect(result.body.singleResult.data.projects.nodes[0].name).toBe('Alpha')
  })

  it('filters projects by repository', async () => {
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "A", repository: "https://github.com/test/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "B", repository: "https://github.com/other/repo" }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          projects(filter: { repository: "https://github.com/test/repo" }) {
            nodes {
              name
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.totalCount).toBe(1)
    expect(result.body.singleResult.data.projects.nodes[0].name).toBe('A')
  })
})

describe('Project Sorting', () => {
  it('sorts projects by name ascending', async () => {
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Charlie", repository: "https://github.com/c/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Alpha", repository: "https://github.com/a/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Beta", repository: "https://github.com/b/repo" }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          projects(sort: { name: "asc" }) {
            nodes {
              name
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.nodes.map((n: any) => n.name)).toEqual(['Alpha', 'Beta', 'Charlie'])
  })

  it('sorts projects by name descending', async () => {
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Charlie", repository: "https://github.com/c/repo" }) { id }
        }
      `,
    })
    await executeOperation({
      query: `
        mutation {
          createProject(input: { name: "Alpha", repository: "https://github.com/a/repo" }) { id }
        }
      `,
    })

    const result = await executeOperation({
      query: `
        query {
          projects(sort: { name: "desc" }) {
            nodes {
              name
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.nodes.map((n: any) => n.name)).toEqual(['Charlie', 'Alpha'])
  })
})

describe('Project Pagination', () => {
  it('paginates with first', async () => {
    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createProject(input: { name: "Project ${i}", repository: "https://github.com/test/repo" }) { id }
          }
        `,
      })
    }

    const result = await executeOperation({
      query: `
        query {
          projects(first: 2) {
            nodes {
              name
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
    expect(result.body.singleResult.data.projects.totalCount).toBe(5)
    expect(result.body.singleResult.data.projects.nodes.length).toBe(2)
    expect(result.body.singleResult.data.projects.pageInfo.hasNextPage).toBe(true)
    expect(result.body.singleResult.data.projects.pageInfo.hasPreviousPage).toBe(false)
  })

  it('paginates with after cursor', async () => {
    for (let i = 0; i < 5; i++) {
      await executeOperation({
        query: `
          mutation {
            createProject(input: { name: "Project ${i}", repository: "https://github.com/test/repo" }) { id }
          }
        `,
      })
    }

    const firstPage = await executeOperation({
      query: `
        query {
          projects(first: 2) {
            nodes {
              name
            }
            pageInfo {
              startCursor
              endCursor
              hasNextPage
            }
          }
        }
      `,
    })

    const endCursor = firstPage.body.singleResult.data.projects.pageInfo.endCursor

    const secondPage = await executeOperation({
      query: `
        query {
          projects(first: 2, after: "${endCursor}") {
            nodes {
              name
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
    expect(secondPage.body.singleResult.data.projects.nodes.length).toBe(2)
    expect(secondPage.body.singleResult.data.projects.pageInfo.hasNextPage).toBe(true)
    expect(secondPage.body.singleResult.data.projects.pageInfo.hasPreviousPage).toBe(true)
  })

  it('returns empty nodes for empty result set', async () => {
    const result = await executeOperation({
      query: `
        query {
          projects {
            nodes {
              name
            }
            pageInfo {
              startCursor
              endCursor
            }
            totalCount
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.totalCount).toBe(0)
    expect(result.body.singleResult.data.projects.nodes.length).toBe(0)
    expect(result.body.singleResult.data.projects.pageInfo.startCursor).toBeNull()
    expect(result.body.singleResult.data.projects.pageInfo.endCursor).toBeNull()
  })

  it('hasNoNextPage at end of results', async () => {
    for (let i = 0; i < 3; i++) {
      await executeOperation({
        query: `
          mutation {
            createProject(input: { name: "Project ${i}", repository: "https://github.com/test/repo" }) { id }
          }
        `,
      })
    }

    const result = await executeOperation({
      query: `
        query {
          projects(first: 3) {
            nodes {
              name
            }
            pageInfo {
              hasNextPage
            }
          }
        }
      `,
    })

    expect(result.body.singleResult.errors).toBeUndefined()
    expect(result.body.singleResult.data.projects.pageInfo.hasNextPage).toBe(false)
  })
})
