import { Request, Response, NextFunction } from 'express'
import pino from 'pino'

const logger = pino({ name: 'http-requests' })

function loggingMiddleware(req: Request, res: Response, next: NextFunction): void {
  const start = Date.now()

  res.on('finish', () => {
    const duration = Date.now() - start
    logger.info({
      method: req.method,
      path: req.path,
      status: res.statusCode,
      duration,
      ip: req.ip,
    }, `${req.method} ${req.path} ${res.statusCode} in ${duration}ms`)
  })

  next()
}

export { loggingMiddleware }
