import { describe, it, expect } from 'vitest';
import { mapMutationError } from './mapMutationError';

describe('mapMutationError', () => {
    it('maps CONCURRENCY_CONFLICT to friendly message', () => {
        const error = new Error('CONCURRENCY_CONFLICT: stale data');
        const result = mapMutationError(error, 'agent task');
        expect(result).toBe('The agent task was modified by another user. Please refresh and try again.');
    });

    it('maps NOT_FOUND to friendly message', () => {
        const error = new Error('NOT_FOUND: id 123 does not exist');
        const result = mapMutationError(error, 'Project');
        expect(result).toBe('Project not found. It may have been deleted.');
    });

    it('passes through generic error messages', () => {
        const error = new Error('Network timeout');
        const result = mapMutationError(error);
        expect(result).toBe('Network timeout');
    });

    it('returns fallback for non-Error objects', () => {
        const result = mapMutationError('string error');
        expect(result).toBe('An unexpected error occurred.');
    });

    it('uses default context label when none provided', () => {
        const error = new Error('CONCURRENCY_CONFLICT');
        const result = mapMutationError(error);
        expect(result).toBe('The item was modified by another user. Please refresh and try again.');
    });
});
