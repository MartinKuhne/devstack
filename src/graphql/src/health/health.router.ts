import express from 'express'
import { checkDatabase } from "./db.checker.js"

export const router: ReturnType<typeof express.Router> = express.Router()

router.get('/', async (_req, res) => {
  const dbCheck = await checkDatabase()

  if (dbCheck.healthy) {
    res.status(200).json({
      status: 'healthy',
      timestamp: new Date().toISOString(),
    })
  } else {
    res.status(503).json({
      status: 'unhealthy',
      timestamp: new Date().toISOString(),
      error: dbCheck.error,
    })
  }
})

export { router as healthRouter }
