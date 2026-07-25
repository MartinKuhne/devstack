import { ApolloClient, InMemoryCache, HttpLink, ApolloLink, Observable } from '@apollo/client';
import { onError } from '@apollo/client/link/error';
import { CombinedGraphQLErrors } from '@apollo/client/errors';
import { toast } from 'react-toastify';
import { logger, createModuleLogger, formatGraphQLError } from '@/lib/logging';
import config from '@/lib/config';

const GRAPHQL_API_URL = config.GRAPHQL_API_URL;

logger.info('ApolloClient: connecting to', GRAPHQL_API_URL);

const apolloLogger = createModuleLogger('ApolloClient');

function createErrorLink(): ApolloLink {
    return onError(({ error, operation }) => {
        if (CombinedGraphQLErrors.is(error)) {
            for (const err of error.errors) {
                const message = err.message || 'An unexpected GraphQL error occurred.';
                apolloLogger.error(`[GraphQL error in ${operation.operationName}]:`, {
                    message,
                    locations: err.locations,
                    path: err.path,
                });
                toast.error(`GraphQL Error: ${message}`);
            }
        } else if (error) {
            apolloLogger.error(`[Network error in ${operation.operationName}]:`, error);
            toast.error('Network Error: Unable to connect to backend server.');
        }
    });
}

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
    const errorLink = createErrorLink();
    const loggingLink = createLoggingLink();
    const link = ApolloLink.from([errorLink, loggingLink, httpLink]);

    const client = new ApolloClient({
        cache: new InMemoryCache(),
        link,
    });
    apolloLogger.debug('ApolloClient created');
    return client;
}

export function logApolloError(error: unknown): void {
    if (CombinedGraphQLErrors.is(error)) {
        for (const gqlErr of error.errors) {
            apolloLogger.error('GraphQL error:', {
                message: gqlErr.message,
                locations: gqlErr.locations,
                path: gqlErr.path,
                extensions: gqlErr.extensions,
            });
        }
    } else {
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
}

export function getApolloClient(): ApolloClient {
    if (!apolloClient) {
        apolloClient = createApolloClient();
    }
    return apolloClient;
}
