import { prisma } from "../config/database.js"
import { buildWhereClause } from "../utils/filtering.js"
import { paginate, PaginatedResult } from "../utils/pagination.js"
import { encryptApiKey, decryptApiKey } from "./encryption.service.js"

export async function createLlm(data: {
  url: string
  model: string
  modelAlias?: string
  apiKey: string
  maxComplexity: number
  maxConcurrency?: number
}): Promise<{
  id: string
  url: string
  model: string
  modelAlias: string | null
  apiKey: string
  maxComplexity: number
  maxConcurrency: number | null
}> {
  const encryptedKey = await encryptApiKey(data.apiKey)

  return prisma.largeLanguageModel.create({
    data: {
      url: data.url,
      model: data.model,
      modelAlias: data.modelAlias ?? null,
      apiKey: encryptedKey,
      maxComplexity: data.maxComplexity,
      maxConcurrency: data.maxConcurrency ?? null,
    },
  })
}

export async function getById(id: string) {
  const model = await prisma.largeLanguageModel.findUnique({
    where: { id },
  })
  if (!model) return null

  return {
    ...model,
    apiKey: await decryptApiKey(model.apiKey),
  }
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
  url: string
  model: string
  modelAlias: string | null
  apiKey: string
  maxComplexity: number
  maxConcurrency: number | null
}>> {
  const where = filter ? buildWhereClause(filter) : {}
  const totalCount = await prisma.largeLanguageModel.count({ where })

  const allModels = await prisma.largeLanguageModel.findMany({
    where,
    orderBy: sort || { id: 'desc' },
  })

  const models = await Promise.all(
    allModels.map(async (m: any) => ({
      ...m,
      apiKey: await decryptApiKey(m.apiKey),
    })),
  )

  return paginate(models, totalCount, first, after)
}

export async function update(
  id: string,
  data: {
    url?: string
    model?: string
    modelAlias?: string
    apiKey?: string
    maxComplexity?: number
    maxConcurrency?: number
  },
): Promise<unknown | null> {
  const existing = await prisma.largeLanguageModel.findUnique({ where: { id } })
  if (!existing) return null

  const updateData: Record<string, unknown> = { ...data }
  if (data.apiKey) {
    updateData.apiKey = await encryptApiKey(data.apiKey)
  }

  return prisma.largeLanguageModel.update({
    where: { id },
    data: updateData,
  })
}

export async function deleteLlm(id: string): Promise<boolean> {
  const existing = await prisma.largeLanguageModel.findUnique({ where: { id } })
  if (!existing) return false

  await prisma.largeLanguageModel.delete({ where: { id } })
  return true
}
