import { ApolloClient, InMemoryCache, HttpLink } from '@apollo/client';
import config from './config';

const httpLink = new HttpLink({
    uri: config.GRAPHQL_API_URL,
});

export const client = new ApolloClient({
    link: httpLink,
    cache: new InMemoryCache(),
});
