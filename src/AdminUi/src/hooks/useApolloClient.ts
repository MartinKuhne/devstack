import { ApolloClient, InMemoryCache, HttpLink } from '@apollo/client';
import { logger } from '@/lib/logging';

const GRAPHQL_API_URL = import.meta.env.VITE_GRAPHQL_API_URL || 'http://localhost:8087/graphql';

logger.info('ApolloClient: connecting to', GRAPHQL_API_URL);

const httpLink = new HttpLink({
    uri: GRAPHQL_API_URL,
});

let apolloClient: ApolloClient | undefined;

function createApolloClient(): ApolloClient {
    const client = new ApolloClient({
        cache: new InMemoryCache(),
        link: httpLink,
    });
    logger.debug('ApolloClient created');
    return client;
}

export function logApolloError(error: unknown): void {
    const err = error as { graphQLErrors?: unknown[]; networkError?: unknown };

    if (err.graphQLErrors?.length) {
        for (const gqlErr of err.graphQLErrors) {
            const e = gqlErr as { message?: string; locations?: unknown; path?: unknown };
            logger.error('GraphQL error:', {
                message: e.message,
                locations: e.locations,
                path: e.path,
            });
        }
    }

    if (err.networkError) {
        logger.error('Network error:', err.networkError);
    }
}

export function getApolloClient(): ApolloClient {
    if (!apolloClient) {
        apolloClient = createApolloClient();
    }
    return apolloClient;
}
