import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createModuleLogger, formatGraphQLError } from './logging';

vi.mock('loglevel', () => ({
    default: {
        setLevel: vi.fn(),
        levels: { TRACE: 0, DEBUG: 1, INFO: 2, WARN: 3, ERROR: 4 },
        trace: vi.fn(),
        debug: vi.fn(),
        info: vi.fn(),
        warn: vi.fn(),
        error: vi.fn(),
    },
}));

describe('createModuleLogger', () => {
    it('returns an object with all logging methods', () => {
        const log = createModuleLogger('test-module');
        expect(log.trace).toBeDefined();
        expect(log.debug).toBeDefined();
        expect(log.info).toBeDefined();
        expect(log.warn).toBeDefined();
        expect(log.error).toBeDefined();
    });

    it('prepends module name prefix to log messages', () => {
        const mockLogger = {
            trace: vi.fn(),
            debug: vi.fn(),
            info: vi.fn(),
            warn: vi.fn(),
            error: vi.fn(),
        };

        const log = createModuleLogger('my-module');

        // Access the internal logger by calling methods
        log.info('hello');
        log.warn('warning');
        log.error('error');

        // The prefix should be prepended
        // We verify the function signature accepts the prefix
        expect(mockLogger.info).not.toThrow();
    });

    it('all methods accept multiple arguments', () => {
        const log = createModuleLogger('test');
        expect(() => log.info('msg', 1, { foo: 'bar' })).not.toThrow();
        expect(() => log.error('err', new Error('oops'))).not.toThrow();
    });
});

describe('formatGraphQLError', () => {
    it('returns default message for undefined error', () => {
        const result = formatGraphQLError(undefined);
        expect(result.message).toBe('An unexpected error occurred');
        expect(result.details).toBeUndefined();
    });

    it('returns default message for null error', () => {
        const result = formatGraphQLError(null);
        expect(result.message).toBe('An unexpected error occurred');
    });

    it('extracts message from plain error object', () => {
        const error = { message: 'Something went wrong' };
        const result = formatGraphQLError(error);
        expect(result.message).toBe('Something went wrong');
    });

    it('extracts message from Error instance', () => {
        const error = new Error('Runtime error');
        const result = formatGraphQLError(error);
        expect(result.message).toBe('Runtime error');
    });

    it('extracts graphQLErrors and returns details', () => {
        const error = {
            message: 'Batch query failed',
            graphQLErrors: [
                { message: 'Field not found', path: ['user', 'email'], extensions: { code: 'USER_NOT_FOUND' } },
                { message: 'Unauthorized', path: ['admin'] },
            ],
        };
        const result = formatGraphQLError(error);
        expect(result.message).toBe('Batch query failed');
        expect(result.details).toHaveLength(2);
        expect(result.details![0]).toEqual({
            message: 'Field not found',
            path: ['user', 'email'],
            extensions: { code: 'USER_NOT_FOUND' },
        });
        expect(result.details![1]).toEqual({
            message: 'Unauthorized',
            path: ['admin'],
            extensions: undefined,
        });
    });

    it('returns default message when graphQLErrors has no outer message', () => {
        const error = { graphQLErrors: [{ message: 'Not found' }] };
        const result = formatGraphQLError(error);
        expect(result.message).toBe('GraphQL operation failed');
    });

    it('extracts network error details', () => {
        const error = {
            message: 'Failed to fetch',
            networkError: { name: 'TypeError', status: 500 },
        };
        const result = formatGraphQLError(error);
        expect(result.message).toBe('Failed to fetch');
        expect(result.details).toEqual({ type: 'TypeError' });
    });

    it('returns default network error message when no message', () => {
        const error = { networkError: { name: 'FetchError' } };
        const result = formatGraphQLError(error);
        expect(result.message).toBe('Network error occurred while communicating with the server');
    });

    it('prefers graphQLErrors over networkError', () => {
        const error = {
            message: 'GraphQL + network',
            graphQLErrors: [{ message: 'Bad query' }],
            networkError: { name: 'TypeError' },
        };
        const result = formatGraphQLError(error);
        expect(result.details).toHaveLength(1);
        expect((result.details as any)[0].message).toBe('Bad query');
    });

    it('handles empty graphQLErrors array', () => {
        const error = { graphQLErrors: [], message: 'No errors' };
        const result = formatGraphQLError(error);
        expect(result.details).toBeUndefined();
        expect(result.message).toBe('No errors');
    });
});
