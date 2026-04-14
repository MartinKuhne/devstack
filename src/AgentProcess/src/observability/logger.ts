import pino from 'pino';
import { loadConfig } from '../config.js';

const config = loadConfig();

const isProduction = config.LOG_LEVEL === 'error' || config.LOG_LEVEL === 'warn';

export const logger = pino({
  level: config.LOG_LEVEL,
  transport: isProduction
    ? undefined
    : {
        target: 'pino-pretty',
        options: {
          colorize: true,
          translateTime: 'SYS:standard',
        },
      },
  formatters: {
    level: (label) => ({ level: label }),
  },
  timestamp: pino.stdTimeFunctions.isoTime,
});

export function createChildLogger(name: string): pino.Logger {
  return logger.child({ module: name });
}
