import { describe, expect, it } from 'vitest';
import {
    DELIVERABLE_STATUS_COLORS,
    PROJECT_STATUS_COLORS,
    AGENT_TASK_STATUS_COLORS,
    getStatusColor,
} from './constants';

describe('getStatusColor', () => {
    it('returns correct color for DELIVERABLE_STATUS_COLORS DRAFT', () => {
        expect(getStatusColor('DRAFT', DELIVERABLE_STATUS_COLORS)).toBe('bg-gray-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS PLANNING', () => {
        expect(getStatusColor('PLANNING', DELIVERABLE_STATUS_COLORS)).toBe('bg-blue-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS READY', () => {
        expect(getStatusColor('READY', DELIVERABLE_STATUS_COLORS)).toBe('bg-green-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS IN_PROGRESS', () => {
        expect(getStatusColor('IN_PROGRESS', DELIVERABLE_STATUS_COLORS)).toBe('bg-yellow-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS IN_REVIEW', () => {
        expect(getStatusColor('IN_REVIEW', DELIVERABLE_STATUS_COLORS)).toBe('bg-purple-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS NEEDS_REVIEW', () => {
        expect(getStatusColor('NEEDS_REVIEW', DELIVERABLE_STATUS_COLORS)).toBe('bg-purple-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', DELIVERABLE_STATUS_COLORS)).toBe('bg-emerald-600');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS FAILED', () => {
        expect(getStatusColor('FAILED', DELIVERABLE_STATUS_COLORS)).toBe('bg-red-500');
    });

    it('returns correct color for DELIVERABLE_STATUS_COLORS REJECTED', () => {
        expect(getStatusColor('REJECTED', DELIVERABLE_STATUS_COLORS)).toBe('bg-gray-600');
    });

    it('returns fallback for unknown status', () => {
        expect(getStatusColor('UNKNOWN', DELIVERABLE_STATUS_COLORS)).toBe('bg-gray-500');
    });

    it('returns fallback for undefined status', () => {
        expect(getStatusColor(undefined, DELIVERABLE_STATUS_COLORS)).toBe('bg-gray-500');
    });

    it('returns correct color for PROJECT_STATUS_COLORS PLANNING', () => {
        expect(getStatusColor('PLANNING', PROJECT_STATUS_COLORS)).toBe('bg-blue-500');
    });

    it('returns correct color for PROJECT_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', PROJECT_STATUS_COLORS)).toBe('bg-emerald-500');
    });

    it('returns correct color for AGENT_TASK_STATUS_COLORS READY', () => {
        expect(getStatusColor('READY', AGENT_TASK_STATUS_COLORS)).toBe('bg-blue-500');
    });

    it('returns correct color for AGENT_TASK_STATUS_COLORS DONE', () => {
        expect(getStatusColor('DONE', AGENT_TASK_STATUS_COLORS)).toBe('bg-green-500');
    });

    it('returns fallback for empty string status', () => {
        expect(getStatusColor('', PROJECT_STATUS_COLORS)).toBe('bg-gray-500');
    });
});

describe('color map exports', () => {
    it('has all expected deliverable status keys', () => {
        const expectedKeys = ['DRAFT', 'PLANNING', 'READY', 'IN_PROGRESS', 'IN_REVIEW', 'NEEDS_REVIEW', 'DONE', 'FAILED', 'REJECTED'];
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

    it('all color values are valid Tailwind bg classes', () => {
        const allMaps = [DELIVERABLE_STATUS_COLORS, PROJECT_STATUS_COLORS, AGENT_TASK_STATUS_COLORS];
        for (const map of allMaps) {
            for (const value of Object.values(map)) {
                expect(value).toMatch(/^bg-(gray|blue|green|yellow|purple|emerald|red)-\d{3}$/);
            }
        }
    });
});
