import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import { gql } from '@apollo/client';
import { toast } from 'react-toastify';
import { getApolloClient, logApolloError } from './useApolloClient';

vi.mock('react-toastify', () => ({
    toast: {
        error: vi.fn(),
        success: vi.fn(),
        info: vi.fn(),
        warn: vi.fn(),
        dismiss: vi.fn(),
    },
}));

const TEST_MUTATION = gql`
    mutation CreateProjectForErrorTest($input: CreateProjectInput!) {
        createProject(input: $input) {
            id
            name
        }
    }
`;

const MUTATION_VARIABLES = { input: { name: 'Test Project', repository: 'repo' } };

/** Builds a `200 OK application/json` GraphQL response. */
function graphqlResponse(body: unknown): Response {
    return new Response(JSON.stringify(body), {
        status: 200,
        headers: { 'content-type': 'application/json' },
    });
}

/** Stubs `fetch` so the Apollo link chain runs without a backend. */
function stubFetch(implementation: () => Promise<Response>): void {
    vi.stubGlobal('fetch', vi.fn(implementation));
}

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

describe('useApolloClient error link', () => {
    const client = getApolloClient();

    beforeEach(() => {
        vi.mocked(toast.error).mockClear();
    });

    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it('toasts every GraphQL error returned by the server', async () => {
        stubFetch(async () =>
            graphqlResponse({
                data: { createProject: null },
                errors: [
                    {
                        message: 'Project name is required',
                        path: ['createProject'],
                        extensions: { code: 'INTERNAL_SERVER_ERROR' },
                    },
                ],
            })
        );

        await expect(
            client.mutate({ mutation: TEST_MUTATION, variables: MUTATION_VARIABLES })
        ).rejects.toThrow(/Project name is required/);

        expect(toast.error).toHaveBeenCalledTimes(1);
        expect(toast.error).toHaveBeenCalledWith('GraphQL Error: Project name is required');
    });

    it('toasts each error when the response carries a CombinedGraphQLErrors list', async () => {
        stubFetch(async () =>
            graphqlResponse({
                data: { createProject: null },
                errors: [
                    { message: 'Repository is required' },
                    { message: 'Name must be 200 characters or less' },
                ],
            })
        );

        await expect(
            client.mutate({ mutation: TEST_MUTATION, variables: MUTATION_VARIABLES })
        ).rejects.toThrow(/Repository is required/);

        expect(toast.error).toHaveBeenCalledTimes(2);
        expect(toast.error).toHaveBeenNthCalledWith(1, 'GraphQL Error: Repository is required');
        expect(toast.error).toHaveBeenNthCalledWith(
            2,
            'GraphQL Error: Name must be 200 characters or less'
        );
    });

    it('falls back to a generic message when a GraphQL error has no message', async () => {
        stubFetch(async () =>
            graphqlResponse({
                data: { createProject: null },
                errors: [{ extensions: { code: 'INTERNAL_SERVER_ERROR' } }],
            })
        );

        await expect(
            client.mutate({ mutation: TEST_MUTATION, variables: MUTATION_VARIABLES })
        ).rejects.toBeTruthy();

        expect(toast.error).toHaveBeenCalledWith(
            'GraphQL Error: An unexpected GraphQL error occurred.'
        );
    });

    it('does not toast when the mutation succeeds', async () => {
        stubFetch(async () =>
            graphqlResponse({ data: { createProject: { id: '1', name: 'Test Project' } } })
        );

        const result = await client.mutate<{ createProject: { id: string } }>({
            mutation: TEST_MUTATION,
            variables: MUTATION_VARIABLES,
        });

        expect(result.data?.createProject.id).toBe('1');
        expect(toast.error).not.toHaveBeenCalled();
    });
});

describe('useApolloClient network error branch', () => {
    const client = getApolloClient();

    beforeEach(() => {
        vi.mocked(toast.error).mockClear();
    });

    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it('toasts the user-facing network error when the backend is unreachable', async () => {
        // What undici throws for a refused connection ("backend not running").
        const unreachable = Object.assign(new TypeError('fetch failed'), {
            cause: Object.assign(new Error('connect ECONNREFUSED 127.0.0.1:8087'), {
                code: 'ECONNREFUSED',
            }),
        });
        stubFetch(async () => {
            throw unreachable;
        });

        await expect(
            client.mutate({ mutation: TEST_MUTATION, variables: MUTATION_VARIABLES })
        ).rejects.toBeTruthy();

        expect(toast.error).toHaveBeenCalledTimes(1);
        expect(toast.error).toHaveBeenCalledWith(
            'Network Error: Unable to connect to backend server.'
        );
    });

    it('toasts the network error for a 5xx response as well', async () => {
        stubFetch(async () =>
            new Response('backend exploded', { status: 503, statusText: 'Service Unavailable' })
        );

        await expect(
            client.mutate({ mutation: TEST_MUTATION, variables: MUTATION_VARIABLES })
        ).rejects.toBeTruthy();

        expect(toast.error).toHaveBeenCalledTimes(1);
        expect(toast.error).toHaveBeenCalledWith(
            'Network Error: Unable to connect to backend server.'
        );
    });
});
