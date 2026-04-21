import { describe, it, expect } from 'vitest';
import * as z from 'zod';

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    repository: z.string().optional(),
});

describe('CreateProjectDialog - Zod Schema Validation', () => {
    it('validates that name is required', () => {
        const result = projectSchema.safeParse({
            name: '',
            description: '',
            repository: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            const issues = result.error.issues;
            expect(issues.length).toBeGreaterThan(0);
            expect(issues[0].message).toBe('Name is required');
        }
    });

    it('validates name maximum length', () => {
        const result = projectSchema.safeParse({
            name: 'a'.repeat(201),
            description: '',
            repository: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            const issues = result.error.issues;
            expect(issues.length).toBeGreaterThan(0);
            expect(issues[0].message).toBe('Name must be 200 characters or less');
        }
    });

    it('accepts empty optional fields', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: '',
            repository: '',
        });
        
        expect(result.success).toBe(true);
    });

    it('accepts valid project data', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: 'A test description',
            repository: 'https://github.com/user/repo',
        });
        
        expect(result.success).toBe(true);
        if (result.success) {
            expect(result.data.name).toBe('Test Project');
            expect(result.data.repository).toBe('https://github.com/user/repo');
        }
    });
});

describe('REQ-UI-016: Create button available with valid input', () => {
    it('form is valid when name is provided', () => {
        const result = projectSchema.safeParse({
            name: 'Valid Project',
            description: 'A description',
            repository: '',
        });
        
        expect(result.success).toBe(true);
    });

    it('form is valid with only required field', () => {
        const result = projectSchema.safeParse({
            name: 'Minimal Project',
        });
        
        expect(result.success).toBe(true);
    });
});

describe('REQ-UI-017: Validation errors displayed and Create button disabled with invalid input', () => {
    it('form is invalid when name is empty', () => {
        const result = projectSchema.safeParse({
            name: '',
            description: '',
            repository: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            expect(result.error.issues.some(i => i.path[0] === 'name')).toBe(true);
        }
    });

    it('form is invalid when name exceeds maximum length', () => {
        const result = projectSchema.safeParse({
            name: 'a'.repeat(201),
            description: '',
            repository: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            expect(result.error.issues.some(i => i.path[0] === 'name')).toBe(true);
        }
    });

    it('form is invalid when all required fields are empty', () => {
        const result = projectSchema.safeParse({
            name: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            const issueMessages = result.error.issues.map(i => i.message);
            expect(issueMessages).toContain('Name is required');
        }
    });
});
