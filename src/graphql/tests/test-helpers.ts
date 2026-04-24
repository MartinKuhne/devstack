import { Pool } from 'pg'
import { GenericContainer } from 'testcontainers'

let pgPool: Pool | undefined
let postgresUrl: string | undefined

async function ensurePostgres(): Promise<Pool> {
  if (pgPool) return pgPool

  const container = await new GenericContainer('postgres:17-alpine')
    .withExposedPorts(5432)
    .withEnvironment({
      POSTGRES_USER: 'postgres',
      POSTGRES_PASSWORD: 'postgres',
      POSTGRES_DB: 'test_graphql',
    })
    .withStartupTimeout(120_000)
    .start()

  const port = container.getMappedPort(5432)
  const host = container.getHost()
  postgresUrl = `postgresql://postgres:postgres@${host}:${port}/test_graphql`
  process.env.DATABASE_URL = postgresUrl

  pgPool = new Pool({ connectionString: postgresUrl })
  await pgPool.query('SELECT 1')

  // Create tables from Prisma schema using Prisma client
  const { PrismaClient } = await import('@prisma/client')
  const prisma = new PrismaClient({ datasourceUrl: postgresUrl })
  await prisma.$connect()

  // Create tables individually (PostgreSQL doesn't allow multi-statement in prepared statements)
  await prisma.$executeRawUnsafe(`CREATE EXTENSION IF NOT EXISTS "uuid-ossp"`)
  await prisma.$executeRawUnsafe(`
    CREATE TABLE IF NOT EXISTS projects (
      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      name VARCHAR(200) NOT NULL,
      description TEXT,
      repository VARCHAR(500) NOT NULL
    )
  `)
  await prisma.$executeRawUnsafe(`
    CREATE TABLE IF NOT EXISTS deliverables (
      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
      type VARCHAR(20) NOT NULL,
      title VARCHAR(200) NOT NULL,
      status VARCHAR(20) NOT NULL DEFAULT 'DRAFT',
      description TEXT,
      acceptance_criteria TEXT,
      execution_plan TEXT,
      agent_feedback TEXT,
      security_impact TEXT,
      performance_impact TEXT,
      test_plan TEXT,
      deployment_plan TEXT,
      blocking TEXT
    )
  `)
  await prisma.$executeRawUnsafe(`
    CREATE TABLE IF NOT EXISTS agent_tasks (
      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      project_id UUID NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
      deliverable_id UUID NOT NULL REFERENCES deliverables(id) ON DELETE CASCADE,
      title VARCHAR(300) NOT NULL,
      status VARCHAR(20) NOT NULL DEFAULT 'READY',
      description TEXT NOT NULL DEFAULT '',
      result TEXT,
      errors TEXT,
      commit_hash VARCHAR(64),
      complexity_rating INTEGER NOT NULL DEFAULT 1,
      depends_on_agent_task_id UUID,
      prompt_tokens INTEGER,
      completion_tokens INTEGER,
      execution_duration_in_seconds INTEGER,
      agent VARCHAR(100)
    )
  `)
  await prisma.$executeRawUnsafe(`
    CREATE TABLE IF NOT EXISTS large_language_models (
      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      url VARCHAR(500) NOT NULL,
      model VARCHAR(200) NOT NULL,
      model_alias VARCHAR(100),
      api_key VARCHAR(1000) NOT NULL,
      max_complexity INTEGER NOT NULL,
      max_concurrency INTEGER DEFAULT 1
    )
  `)
  await prisma.$disconnect()

  return pgPool
}

async function cleanDatabase(): Promise<void> {
  if (!pgPool) return
  await pgPool.query('DELETE FROM agent_tasks')
  await pgPool.query('DELETE FROM deliverables')
  await pgPool.query('DELETE FROM large_language_models')
  await pgPool.query('DELETE FROM projects')
}

async function closeDatabase(): Promise<void> {
  if (pgPool) {
    await pgPool.end()
    pgPool = undefined
  }
}

export { ensurePostgres, cleanDatabase, closeDatabase, postgresUrl }
