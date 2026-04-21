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

export function createModuleLogger(moduleName: string) {
    const prefix = `[${moduleName}]`;
    return {
        trace: (...args: unknown[]) => logger.trace(prefix, ...args),
        debug: (...args: unknown[]) => logger.debug(prefix, ...args),
        info: (...args: unknown[]) => logger.info(prefix, ...args),
        warn: (...args: unknown[]) => logger.warn(prefix, ...args),
        error: (...args: unknown[]) => logger.error(prefix, ...args),
    };
}

export function formatGraphQLError(error: unknown): { message: string; details?: unknown } {
    const err = error as Record<string, unknown> | undefined;
    const message = err?.message as string | undefined;
    const graphQLErrors = err?.graphQLErrors as Record<string, unknown>[] | undefined;
    const networkError = err?.networkError as Record<string, unknown> | undefined;

    if (graphQLErrors && graphQLErrors.length > 0) {
        const details = graphQLErrors.map((e: Record<string, unknown>) => ({
            message: e.message,
            path: e.path,
            extensions: e.extensions,
        }));
        return { message: message ?? 'GraphQL operation failed', details };
    }

    if (networkError) {
        return {
            message: message ?? 'Network error occurred while communicating with the server',
            details: { type: (networkError as Record<string, unknown>).name },
        };
    }

    return { message: message ?? 'An unexpected error occurred' };
}

export function setupGlobalErrorHandlers() {
    window.onerror = (message, source, lineno, colno, error) => {
        logger.error('Uncaught error:', {
            message: message instanceof Error ? message.message : String(message),
            source,
            lineno,
            colno,
            stack: error?.stack || undefined,
        });
        return false;
    };

    window.onunhandledrejection = (event) => {
        const reason = event.reason;
        if (reason instanceof Error) {
            logger.error('Unhandled promise rejection:', {
                message: reason.message,
                stack: reason.stack,
            });
        } else {
            logger.error('Unhandled promise rejection:', reason);
        }
    };
}
