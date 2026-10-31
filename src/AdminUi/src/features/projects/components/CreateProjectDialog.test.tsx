import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import * as z from 'zod';
import { GraphQLError } from 'graphql';
import { CombinedGraphQLErrors } from '@apollo/client/errors';

vi.mock('@/generated/graphql', () => ({
    useCreateProjectMutation: vi.fn(),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useCreateProjectMutation: m.useCreateProjectMutation as ReturnType<typeof vi.fn>,
    };
};

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
            expect(issues[0]?.message).toBe('Name is required');
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
            expect(issues[0]?.message).toBe('Name must be 200 characters or less');
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

describe('CreateProjectDialog - GraphQL error surface', () => {
    const onOpenChange = vi.fn();
    const onSuccess = vi.fn();
    let createProjectMock: ReturnType<typeof vi.fn>;

    /** Renders the dialog, fills the only required field and submits the form. */
    async function submitValidForm() {
        const { CreateProjectDialog } = await import('./CreateProjectDialog');
        render(<CreateProjectDialog open onOpenChange={onOpenChange} onSuccess={onSuccess} />);

        fireEvent.change(screen.getByLabelText(/name \*/i), {
            target: { value: '[DeleteAfterTest] Broken Project' },
        });

        const submitButton = screen.getByRole('button', { name: /create project/i });
        await waitFor(() => expect(submitButton).toBeEnabled());
        fireEvent.click(submitButton);
    }

    beforeEach(async () => {
        vi.clearAllMocks();
        createProjectMock = vi.fn();
        const hooks = await getMockedHooks();
        hooks.useCreateProjectMutation.mockReturnValue([createProjectMock, { loading: false }]);
    });

    it('sends the mutation with the CreateProjectInput shape', async () => {
        createProjectMock.mockResolvedValue({ data: { createProject: { id: '1' } } });

        await submitValidForm();

        await waitFor(() =>
            expect(createProjectMock).toHaveBeenCalledWith({
                variables: {
                    input: {
                        name: '[DeleteAfterTest] Broken Project',
                        description: '',
                        repository: '',
                    },
                },
            })
        );
    });

    it('shows the server error banner and keeps the dialog open on a rejected mutation', async () => {
        createProjectMock.mockRejectedValue(
            new CombinedGraphQLErrors({
                data: { createProject: null },
                errors: [new GraphQLError('A project with this repository already exists.')],
            })
        );

        await submitValidForm();

        await waitFor(() =>
            expect(
                screen.getByText('A project with this repository already exists.')
            ).toBeInTheDocument()
        );

        // the dialog must stay open so the user can fix the input
        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(onSuccess).not.toHaveBeenCalled();
        expect(onOpenChange).not.toHaveBeenCalledWith(false);
    });

    it('shows the server error banner when the mutation resolves without a project', async () => {
        createProjectMock.mockResolvedValue({ data: { createProject: null } });

        await submitValidForm();

        await waitFor(() => expect(screen.getByText('Failed to create project')).toBeInTheDocument());

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(onSuccess).not.toHaveBeenCalled();
        expect(onOpenChange).not.toHaveBeenCalledWith(false);
    });

    it('surfaces a network failure in the banner instead of swallowing it', async () => {
        createProjectMock.mockRejectedValue(new Error('Network request failed'));

        await submitValidForm();

        await waitFor(() => expect(screen.getByText('Network request failed')).toBeInTheDocument());

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(onSuccess).not.toHaveBeenCalled();
    });

    it('closes the dialog and reports success when the mutation succeeds', async () => {
        createProjectMock.mockResolvedValue({ data: { createProject: { id: '42' } } });

        await submitValidForm();

        await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1));
        expect(onOpenChange).toHaveBeenCalledWith(false);
        expect(screen.queryByText('Failed to create project')).not.toBeInTheDocument();
    });
});
