import { prisma } from "../config/database.js"

export async function checkDatabase(): Promise<{ healthy: boolean; error?: string }> {
  try {
    await prisma.$queryRaw`SELECT 1`
    return { healthy: true }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Unknown error'
    return { healthy: false, error: errorMessage }
  }
}
