export class InMemoryStore {
  private tables: Map<string, any[]> = new Map()

  private getTable(name: string): any[] {
    if (!this.tables.has(name)) {
      this.tables.set(name, [])
    }
    return this.tables.get(name)!
  }

  create(table: string, data: any): any {
    const entities = this.getTable(table)
    const entity = { ...data, id: data.id || crypto.randomUUID() }
    
    // Set default values for required fields
    if (table === 'agentTask' && !entity.status) {
      entity.status = 'READY'
    }
    
    entities.push(entity)
    return entity
  }

  findUnique(table: string, args: any): any | null {
    const where = typeof args === 'object' && 'where' in args ? args.where : args
    const include = typeof args === 'object' && 'include' in args ? args.include : undefined
    const entities = this.getTable(table)
    const entity = entities.find(e => e.id === where?.id) || null

    if (entity && include?.project) {
      const project = this.getTable('project').find(p => p.id === entity.projectId) || null
      return { ...entity, project }
    }

    return entity
  }

  findMany(table: string, args: any = {}): any[] {
    let entities = [...this.getTable(table)]

    if (args.where && typeof args.where === 'object') {
      entities = entities.filter(entity => {
        return Object.entries(args.where).every(([key, value]) => {
          if (typeof value === 'object' && value !== null && 'in' in value && Array.isArray(value.in)) {
            return value.in.includes(entity[key])
          }
          return entity[key] === value
        })
      })
    }

    if (args.orderBy && typeof args.orderBy === 'object') {
      const entries = Object.entries(args.orderBy)
      if (entries.length > 0) {
        const [field, direction] = entries[0]
        entities.sort((a, b) => {
          const aVal = a[field] ?? ''
          const bVal = b[field] ?? ''
          if (typeof aVal === 'number' && typeof bVal === 'number') {
            return direction === 'asc' ? aVal - bVal : bVal - aVal
          }
          return direction === 'asc'
            ? String(aVal).localeCompare(String(bVal))
            : String(bVal).localeCompare(String(aVal))
        })
      }
    }

    return entities
  }

  count(table: string, args: any = {}): number {
    return this.findMany(table, args).length
  }

  update(table: string, args: any): any | null {
    const entities = this.getTable(table)
    const index = entities.findIndex(e => e.id === args.where?.id)
    if (index === -1) return null
    entities[index] = { ...entities[index], ...args.data }
    return entities[index]
  }

  delete(table: string, args: any): boolean {
    const entities = this.getTable(table)
    const where = typeof args === 'object' && 'where' in args ? args.where : args
    const index = entities.findIndex(e => e.id === where?.id)
    if (index === -1) return false
    entities.splice(index, 1)
    return true
  }

  deleteMany(table: string): number {
    const entities = this.getTable(table)
    const count = entities.length
    entities.length = 0
    return count
  }

  clear() {
    for (const key of this.tables.keys()) {
      this.tables.set(key, [])
    }
  }
}

export function createMockPrisma(store: InMemoryStore) {
  const entities = ['project', 'deliverable', 'agentTask', 'largeLanguageModel']

  function createModel(entityName: string) {
    return {
      create: (args: any) => store.create(entityName, args?.data ?? args),
      findUnique: (args: any) => store.findUnique(entityName, args),
      findMany: (args: any) => store.findMany(entityName, args),
      count: (args: any) => store.count(entityName, args),
      update: (args: any) => store.update(entityName, args),
      delete: (args: any) => store.delete(entityName, args),
      deleteMany: () => store.deleteMany(entityName),
    }
  }

  const client: any = {}
  for (const entity of entities) {
    client[entity] = createModel(entity)
  }
  client.$connect = async () => {}
  client.$disconnect = async () => {}

  return client
}
