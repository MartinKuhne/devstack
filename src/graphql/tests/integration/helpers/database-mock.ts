import { InMemoryStore, createMockPrisma } from './mock-prisma.js'

let _store: InMemoryStore | null = null

export function getStore(): InMemoryStore {
  if (!_store) {
    throw new Error('Store not initialized. Call vi.mock() first.')
  }
  return _store
}

export function createDatabaseMock() {
  _store = new InMemoryStore()
  const mockPrisma = createMockPrisma(_store)
  return {
    prisma: mockPrisma,
    connectDatabase: async () => {},
    disconnectDatabase: async () => {},
  }
}
