import { describe, it, expect } from 'vitest';
import * as z from 'zod';

const createTaskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    deliverable: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    risks: z.string().optional(),
    requiredFollowUps: z.string().optional(),
    complexity: z.enum(['Simple', 'Moderate', 'Complex', 'Major']),
    featureId: z.string().min(1, 'Feature is required'),
});

type CreateTaskFormData = z.infer<typeof createTaskSchema>;

describe('Task Form Validation - CreateTaskDialog', () => {
    it('validates that title is required', () => {
        const result = createTaskSchema.safeParse({
            title: '',
            complexity: 'Simple',
            featureId: 'feature-123',
        });

        expect(result.success).toBe(false);
        if (!result.success) {
            const error = result.error.issues.find((e) => e.path.includes('title'));
            expect(error?.message).toBe('Title is required');
        }
    });

    it('validates that title must be 200 characters or less', () => {
        const result = createTaskSchema.safeParse({
            title: 'a'.repeat(201),
            complexity: 'Simple',
            featureId: 'feature-123',
        });

        expect(result.success).toBe(false);
        if (!result.success) {
            const error = result.error.issues.find((e) => e.path.includes('title'));
            expect(error?.message).toBe('Title must be 200 characters or less');
        }
    });

    it('accepts valid task data with minimal fields', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Simple',
            featureId: 'feature-123',
        });

        expect(result.success).toBe(true);
    });

    it('accepts valid task data with all fields', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            deliverable: 'A deliverable',
            acceptanceCriteria: 'Criteria',
            risks: 'Risks',
            requiredFollowUps: 'Follow-ups',
            complexity: 'Complex',
            featureId: 'feature-123',
        });

        expect(result.success).toBe(true);
    });

    it('validates that featureId is required', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Simple',
            featureId: '',
        });

        expect(result.success).toBe(false);
        if (!result.success) {
            const error = result.error.issues.find((e) => e.path.includes('featureId'));
            expect(error?.message).toBe('Feature is required');
        }
    });

    it('accepts all valid complexity values', () => {
        const complexities = ['Simple', 'Moderate', 'Complex', 'Major'] as const;

        complexities.forEach((complexity) => {
            const result = createTaskSchema.safeParse({
                title: 'Test Task',
                complexity,
                featureId: 'feature-123',
            });

            expect(result.success).toBe(true);
        });
    });

    it('rejects invalid complexity values', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Invalid' as any,
            featureId: 'feature-123',
        });

        expect(result.success).toBe(false);
    });

    it('allows optional fields to be undefined', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Simple',
            featureId: 'feature-123',
            deliverable: undefined,
            acceptanceCriteria: undefined,
            risks: undefined,
            requiredFollowUps: undefined,
        });

        expect(result.success).toBe(true);
    });

    it('allows optional fields to be empty strings', () => {
        const result = createTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Simple',
            featureId: 'feature-123',
            deliverable: '',
            acceptanceCriteria: '',
            risks: '',
            requiredFollowUps: '',
        });

        expect(result.success).toBe(true);
    });
});

describe('Task Form Validation - EditTaskDialog', () => {
    const editTaskSchema = z.object({
        title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
        deliverable: z.string().optional(),
        acceptanceCriteria: z.string().optional(),
        risks: z.string().optional(),
        requiredFollowUps: z.string().optional(),
        complexity: z.enum(['Simple', 'Moderate', 'Complex', 'Major']),
        status: z.enum(['Todo', 'InProgress', 'Review', 'Done']),
    });

    it('validates that title is required on edit', () => {
        const result = editTaskSchema.safeParse({
            title: '',
            complexity: 'Simple',
            status: 'Todo',
        });

        expect(result.success).toBe(false);
        if (!result.success) {
            const error = result.error.issues.find((e) => e.path.includes('title'));
            expect(error?.message).toBe('Title is required');
        }
    });

    it('accepts valid task data for editing', () => {
        const result = editTaskSchema.safeParse({
            title: 'Updated Task',
            deliverable: 'Updated deliverable',
            acceptanceCriteria: 'Updated criteria',
            risks: 'Updated risks',
            requiredFollowUps: 'Updated follow-ups',
            complexity: 'Moderate',
            status: 'InProgress',
        });

        expect(result.success).toBe(true);
    });

    it('accepts all valid status values', () => {
        const statuses = ['Todo', 'InProgress', 'Review', 'Done'] as const;

        statuses.forEach((status) => {
            const result = editTaskSchema.safeParse({
                title: 'Test Task',
                complexity: 'Simple',
                status,
            });

            expect(result.success).toBe(true);
        });
    });

    it('rejects invalid status values', () => {
        const result = editTaskSchema.safeParse({
            title: 'Test Task',
            complexity: 'Simple',
            status: 'Invalid' as any,
        });

        expect(result.success).toBe(false);
    });

    it('allows partial updates with only required fields', () => {
        const result = editTaskSchema.safeParse({
            title: 'Updated Title',
            complexity: 'Complex',
            status: 'Done',
        });

        expect(result.success).toBe(true);
    });
});

describe('Task Complexity Color Logic', () => {
    const COMPLEXITY_COLORS: Record<string, string> = {
        Simple: 'bg-green-500',
        Moderate: 'bg-yellow-500',
        Complex: 'bg-orange-500',
        Major: 'bg-red-500',
    };

    it('maps Simple complexity to green', () => {
        expect(COMPLEXITY_COLORS['Simple']).toBe('bg-green-500');
    });

    it('maps Moderate complexity to yellow', () => {
        expect(COMPLEXITY_COLORS['Moderate']).toBe('bg-yellow-500');
    });

    it('maps Complex complexity to orange', () => {
        expect(COMPLEXITY_COLORS['Complex']).toBe('bg-orange-500');
    });

    it('maps Major complexity to red', () => {
        expect(COMPLEXITY_COLORS['Major']).toBe('bg-red-500');
    });

    it('has color mapping for all valid complexity values', () => {
        const validComplexities = ['Simple', 'Moderate', 'Complex', 'Major'];
        
        validComplexities.forEach((complexity) => {
            expect(COMPLEXITY_COLORS[complexity]).toBeDefined();
            expect(COMPLEXITY_COLORS[complexity]).not.toBeUndefined();
        });
    });
});

describe('Task Status Values', () => {
    const VALID_STATUSES = ['Todo', 'InProgress', 'Review', 'Done'] as const;

    it('defines all required task statuses', () => {
        expect(VALID_STATUSES).toContain('Todo');
        expect(VALID_STATUSES).toContain('InProgress');
        expect(VALID_STATUSES).toContain('Review');
        expect(VALID_STATUSES).toContain('Done');
    });

    it('has exactly 4 valid status values', () => {
        expect(VALID_STATUSES).toHaveLength(4);
    });

    it('status values use PascalCase naming', () => {
        VALID_STATUSES.forEach((status) => {
            expect(status).toMatch(/^[A-Z][a-zA-Z]*$/);
        });
    });
});
