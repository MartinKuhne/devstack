import { ApolloClient, InMemoryCache, HttpLink } from '@apollo/client';

const httpLink = new HttpLink({
    uri: import.meta.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql',
});

let apolloClient: ApolloClient | undefined;

function createApolloClient() {
    return new ApolloClient({
        cache: new InMemoryCache(),
        link: httpLink,
    });
}

export function getApolloClient() {
    if (!apolloClient) {
        apolloClient = createApolloClient();
    }
    return apolloClient;
}
