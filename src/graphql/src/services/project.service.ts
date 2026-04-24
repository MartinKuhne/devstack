import { prisma } from "../config/database.js"
import { Prisma } from '@prisma/client'
import { buildWhereClause } from "../utils/filtering.js"
import { paginate, PaginatedResult } from "../utils/pagination.js"

export async function createProject(data: {
  name: string
  repository: string
  description?: string
}): Promise<{ id: string; name: string; description: string | null; repository: string }> {
  return prisma.project.create({
    data: {
      name: data.name,
      repository: data.repository,
      description: data.description ?? null,
    },
  })
}

export async function getById(id: string): Promise<{
  id: string
  name: string
  description: string | null
  repository: string
} | null> {
  return prisma.project.findUnique({
    where: { id },
  })
}

export async function getAll({
  first,
  after,
  filter,
  sort,
}: {
  first?: number
  after?: string
  filter?: Record<string, unknown>
  sort?: Record<string, string>
} = {}): Promise<PaginatedResult<{
  id: string
  name: string
  description: string | null
  repository: string
}>> {
  const where = filter ? buildWhereClause(filter) : {}
  const totalCount = await prisma.project.count({ where })

  const allProjects = await prisma.project.findMany({
    where,
    orderBy: sort || { id: 'desc' },
  })

  return paginate(allProjects, totalCount, first, after)
}

export async function update(
  id: string,
  data: {
    name?: string
    repository?: string
    description?: string
  },
): Promise<{ id: string; name: string; description: string | null; repository: string } | null> {
  const existing = await prisma.project.findUnique({ where: { id } })
  if (!existing) return null

  const updateData: Prisma.ProjectUpdateInput = {}
  if (data.name !== undefined) updateData.name = data.name
  if (data.repository !== undefined) updateData.repository = data.repository
  if (data.description !== undefined) updateData.description = data.description

  return prisma.project.update({
    where: { id },
    data: updateData,
  })
}

export async function deleteProject(id: string): Promise<boolean> {
  const existing = await prisma.project.findUnique({ where: { id } })
  if (!existing) return false

  await prisma.project.delete({ where: { id } })
  return true
}
