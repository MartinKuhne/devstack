import log from 'loglevel';

const envLogLevel = import.meta.env.VITE_LOG_LEVEL?.toLowerCase();
const validLevels = ['trace', 'debug', 'info', 'warn', 'error'];
if (envLogLevel && validLevels.includes(envLogLevel)) {
    log.setLevel(log.levels[envLogLevel as keyof typeof log.levels] ?? log.levels.INFO);
} else {
    log.setLevel(log.levels.INFO);
}

export const logger = {
    trace: log.trace.bind(log),
    debug: log.debug.bind(log),
    info: log.info.bind(log),
    warn: log.warn.bind(log),
    error: log.error.bind(log),
};

export function setupGlobalErrorHandlers() {
    window.onerror = (message, source, lineno, colno, error) => {
        logger.error('Uncaught error:', { message, source, lineno, colno, error: error?.stack || error });
        return false;
    };

    window.onunhandledrejection = (event) => {
        logger.error('Unhandled promise rejection:', event.reason);
    };
}