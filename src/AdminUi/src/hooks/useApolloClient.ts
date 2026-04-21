import { ApolloClient, InMemoryCache, HttpLink, ApolloLink, Observable } from '@apollo/client';
import { logger, createModuleLogger, formatGraphQLError } from '@/lib/logging';

const GRAPHQL_API_URL = import.meta.env.VITE_GRAPHQL_API_URL || 'http://localhost:8087/graphql';

logger.info('ApolloClient: connecting to', GRAPHQL_API_URL);

const apolloLogger = createModuleLogger('ApolloClient');

function createLoggingLink(): ApolloLink {
    return new ApolloLink((operation, forward) => {
        const operationName = operation.operationName || 'anonymous';
        const operationType = operation.operationType;

        apolloLogger.info(`${operationType} ${operationName} started`);

        const startTime = Date.now();

        return new Observable((observe) => {
            forward(operation).subscribe({
                next: (result) => {
                    const duration = Date.now() - startTime;
                    const hasErrors = result.errors && result.errors.length > 0;

                    if (hasErrors) {
                        apolloLogger.error(
                            `${operationType} ${operationName} completed with errors`,
                            {
                                duration,
                                errors: result.errors,
                            }
                        );
                    } else {
                        apolloLogger.debug(
                            `${operationType} ${operationName} completed successfully`,
                            {
                                duration,
                            }
                        );
                    }

                    observe.next(result);
                },
                error: (error) => {
                    const duration = Date.now() - startTime;
                    apolloLogger.error(`${operationType} ${operationName} failed`, {
                        duration,
                        error: formatGraphQLError(error),
                    });
                    observe.error(error);
                },
                complete: () => {
                    apolloLogger.debug(`${operationType} ${operationName} completed`);
                    observe.complete();
                },
            });
        });
    });
}

const httpLink = new HttpLink({
    uri: GRAPHQL_API_URL,
});

let apolloClient: ApolloClient | undefined;

function createApolloClient(): ApolloClient {
    const loggingLink = createLoggingLink();
    const link = loggingLink.concat(httpLink);

    const client = new ApolloClient({
        cache: new InMemoryCache(),
        link,
    });
    apolloLogger.debug('ApolloClient created');
    return client;
}

export function logApolloError(error: unknown): void {
    const err = error as { graphQLErrors?: unknown[]; networkError?: unknown };

    if (err.graphQLErrors?.length) {
        for (const gqlErr of err.graphQLErrors) {
            const e = gqlErr as {
                message?: string;
                locations?: unknown;
                path?: unknown;
                extensions?: unknown;
            };
            apolloLogger.error('GraphQL error:', {
                message: e.message,
                locations: e.locations,
                path: e.path,
                extensions: e.extensions,
            });
        }
    }

    if (err.networkError) {
        apolloLogger.error('Network error:', err.networkError);
    }
}

export function getApolloClient(): ApolloClient {
    if (!apolloClient) {
        apolloClient = createApolloClient();
    }
    return apolloClient;
}
