import { describe, expect, it } from 'vitest';
import { getApolloClient, logApolloError } from './useApolloClient';

describe('useApolloClient', () => {
    it('creates and returns singleton ApolloClient instance', () => {
        const client1 = getApolloClient();
        const client2 = getApolloClient();

        expect(client1).toBeDefined();
        expect(client2).toBe(client1);
    });

    it('handles logApolloError without throwing', () => {
        expect(() => {
            logApolloError({
                graphQLErrors: [{ message: 'Test GraphQL error' }],
                networkError: new Error('Network failure'),
            });
        }).not.toThrow();
    });
});
