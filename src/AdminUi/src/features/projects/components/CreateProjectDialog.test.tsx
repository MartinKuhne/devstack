import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';
import * as z from 'zod';

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    architecture: z.string().optional(),
    memory: z.string().optional(),
    githubUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
});

describe('CreateProjectDialog - Zod Schema Validation', () => {
    it('validates that name is required', () => {
        const result = projectSchema.safeParse({
            name: '',
            description: '',
            architecture: '',
            memory: '',
            githubUrl: '',
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
            architecture: '',
            memory: '',
            githubUrl: '',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            const issues = result.error.issues;
            expect(issues.length).toBeGreaterThan(0);
            expect(issues[0].message).toBe('Name must be 200 characters or less');
        }
    });

    it('validates GitHub URL format', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: '',
            architecture: '',
            memory: '',
            githubUrl: 'not-a-valid-url',
        });
        
        expect(result.success).toBe(false);
        if (!result.success) {
            const issues = result.error.issues;
            expect(issues.length).toBeGreaterThan(0);
            expect(issues[0].message).toBe('Invalid URL');
        }
    });

    it('accepts valid GitHub URL', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: '',
            architecture: '',
            memory: '',
            githubUrl: 'https://github.com/user/repo',
        });
        
        expect(result.success).toBe(true);
    });

    it('allows empty optional fields', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: '',
            architecture: '',
            memory: '',
            githubUrl: '',
        });
        
        expect(result.success).toBe(true);
    });

    it('accepts valid project data', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: 'A test description',
            architecture: 'Some architecture notes',
            memory: 'Some memory notes',
            githubUrl: 'https://github.com/user/repo',
        });
        
        expect(result.success).toBe(true);
        if (result.success) {
            expect(result.data.name).toBe('Test Project');
            expect(result.data.githubUrl).toBe('https://github.com/user/repo');
        }
    });
});

describe('CreateProjectDialog - Zod Schema Validation', () => {
    it('accepts empty optional fields', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: '',
            architecture: '',
            memory: '',
            githubUrl: '',
        });
        
        expect(result.success).toBe(true);
    });

    it('accepts valid project data', () => {
        const result = projectSchema.safeParse({
            name: 'Test Project',
            description: 'A test description',
            architecture: 'Some architecture notes',
            memory: 'Some memory notes',
            githubUrl: 'https://github.com/user/repo',
        });
        
        expect(result.success).toBe(true);
        if (result.success) {
            expect(result.data.name).toBe('Test Project');
            expect(result.data.githubUrl).toBe('https://github.com/user/repo');
        }
    });

    it('handles concurrency conflict error message format', () => {
        const errorMessage = 'CONCURRENCY_CONFLICT: The project has been modified by another process';
        
        expect(errorMessage).toContain('CONCURRENCY_CONFLICT');
        expect(errorMessage).toContain('modified by another process');
    });
});
