import { describe, expect, it } from 'vitest';
import {
    DELIVERABLE_STATUS_COLORS,
    PROJECT_STATUS_COLORS,
    AGENT_TASK_STATUS_COLORS,
    getStatusColor,
    getStatusVariant,
    DELIVERABLE_STATUS_VARIANTS,
} from './constants';

describe('getStatusColor', () => {
    it('returns correct color for DELIVERABLE_STATUS_COLORS DRAFT', () => {
        expect(getStatusColor('DRAFT', DELIVERABLE_STATUS_COLORS)).toBe('bg-muted');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', DELIVERABLE_STATUS_COLORS)).toBe('bg-success');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS FAILED', () => {
        expect(getStatusColor('FAILED', DELIVERABLE_STATUS_COLORS)).toBe('bg-destructive');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS IMPLEMENT', () => {
        expect(getStatusColor('IMPLEMENT', DELIVERABLE_STATUS_COLORS)).toBe('bg-warning');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS REJECTED', () => {
        expect(getStatusColor('REJECTED', DELIVERABLE_STATUS_COLORS)).toBe('bg-destructive');
    });

    it('returns fallback for unknown status', () => {
        expect(getStatusColor('UNKNOWN', DELIVERABLE_STATUS_COLORS)).toBe('bg-muted');
    });

    it('returns fallback for undefined status', () => {
        expect(getStatusColor(undefined, DELIVERABLE_STATUS_COLORS)).toBe('bg-muted');
    });

    it('returns correct color for PROJECT_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', PROJECT_STATUS_COLORS)).toBe('bg-success');
    });

    it('returns correct color for AGENT_TASK_STATUS_COLORS READY', () => {
        expect(getStatusColor('READY', AGENT_TASK_STATUS_COLORS)).toBe('bg-primary');
    });

    it('returns correct color for AGENT_TASK_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', AGENT_TASK_STATUS_COLORS)).toBe('bg-success');
    });

    it('returns fallback for empty string status', () => {
        expect(getStatusColor('', PROJECT_STATUS_COLORS)).toBe('bg-muted');
    });
});

describe('getStatusVariant', () => {
    it('returns correct variant for deliverable status', () => {
        expect(getStatusVariant('DONE', DELIVERABLE_STATUS_VARIANTS)).toBe('success');
        expect(getStatusVariant('FAILED', DELIVERABLE_STATUS_VARIANTS)).toBe('destructive');
        expect(getStatusVariant('DRAFT', DELIVERABLE_STATUS_VARIANTS)).toBe('secondary');
    });

    it('returns fallback for undefined', () => {
        expect(getStatusVariant(undefined, DELIVERABLE_STATUS_VARIANTS)).toBe('secondary');
    });
});

describe('color map exports', () => {
    it('has all expected deliverable status keys', () => {
        const expectedKeys = ['DRAFT', 'DESIGN', 'PLAN', 'IMPLEMENT', 'MERGE', 'DEPLOY', 'TEST', 'DONE', 'NEEDS_REVIEW', 'FAILED', 'REJECTED'];
        for (const key of expectedKeys) {
            expect(DELIVERABLE_STATUS_COLORS).toHaveProperty(key);
        }
    });

    it('has all expected project status keys', () => {
        const expectedKeys = ['PLANNING', 'READY', 'IN_PROGRESS', 'NEEDS_REVIEW', 'DONE', 'FAILED', 'REJECTED'];
        for (const key of expectedKeys) {
            expect(PROJECT_STATUS_COLORS).toHaveProperty(key);
        }
    });

    it('has all expected agent task status keys', () => {
        const expectedKeys = ['READY', 'IN_PROGRESS', 'NEEDS_REVIEW', 'DONE', 'FAILED', 'REJECTED'];
        for (const key of expectedKeys) {
            expect(AGENT_TASK_STATUS_COLORS).toHaveProperty(key);
        }
    });

    it('all color values use theme token classes', () => {
        const allMaps = [DELIVERABLE_STATUS_COLORS, PROJECT_STATUS_COLORS, AGENT_TASK_STATUS_COLORS];
        const validTokens = ['bg-primary', 'bg-secondary', 'bg-muted', 'bg-destructive', 'bg-success', 'bg-warning'];
        for (const map of allMaps) {
            for (const value of Object.values(map)) {
                expect(validTokens).toContain(value);
            }
        }
    });

    it('destructive is only used for FAILED and REJECTED', () => {
        const allMaps = [DELIVERABLE_STATUS_COLORS, PROJECT_STATUS_COLORS, AGENT_TASK_STATUS_COLORS];
        for (const map of allMaps) {
            for (const [key, value] of Object.entries(map)) {
                if (value === 'bg-destructive') {
                    expect(['FAILED', 'REJECTED']).toContain(key);
                }
            }
        }
    });
});
