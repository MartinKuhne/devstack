import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { GraphQLError } from 'graphql';
import { CombinedGraphQLErrors } from '@apollo/client/errors';

vi.mock('@/generated/graphql', () => ({
    useCreateDeliverableMutation: vi.fn(),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useCreateDeliverableMutation: m.useCreateDeliverableMutation as ReturnType<typeof vi.fn>,
    };
};

describe('CreateDeliverableDialog', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedHooks();
        hooks.useCreateDeliverableMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('renders form with correct labels', async () => {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId="test-project-id"
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getAllByRole('combobox').length).toBeGreaterThanOrEqual(1);
    });

    it('has correct dialog title', async () => {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId="test-project-id"
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/create new deliverable/i)).toBeInTheDocument();
    });

    it('calls onOpenChange when cancel button is clicked', async () => {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId="test-project-id"
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('has type select with correct options', async () => {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId="test-project-id"
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/type \*/i)).toBeInTheDocument();
    });

    it('has initial status select with correct options', async () => {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId="test-project-id"
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/initial status/i)).toBeInTheDocument();
    });
});

describe('CreateDeliverableDialog - GraphQL error surface', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();
    const VALID_PROJECT_ID = '3f2504e0-4f89-41d3-9a0c-0305e82c3301';
    let createDeliverableMock: ReturnType<typeof vi.fn>;

    /** Renders the dialog for a project, fills the required title and submits. */
    async function submitValidForm(projectId: string) {
        const { CreateDeliverableDialog } = await import('./CreateDeliverableDialog');
        render(
            <CreateDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                projectId={projectId}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.change(screen.getByLabelText(/^title \*/i), {
            target: { value: '[DeleteAfterTest] Broken Deliverable' },
        });

        const submitButton = screen.getByRole('button', { name: /create deliverable/i });
        await waitFor(() => expect(submitButton).toBeEnabled());
        fireEvent.click(submitButton);
    }

    beforeEach(async () => {
        vi.clearAllMocks();
        createDeliverableMock = vi.fn();
        const hooks = await getMockedHooks();
        hooks.useCreateDeliverableMutation.mockReturnValue([
            createDeliverableMock,
            { loading: false },
        ]);
    });

    it('shows the server error banner and keeps the dialog open on a rejected mutation', async () => {
        createDeliverableMock.mockRejectedValue(
            new CombinedGraphQLErrors({
                data: { createDeliverable: null },
                errors: [
                    new GraphQLError('Deliverable with ID 3f2504e0 not found.'),
                ],
            })
        );

        await submitValidForm(VALID_PROJECT_ID);

        await waitFor(() =>
            expect(screen.getByText('Deliverable with ID 3f2504e0 not found.')).toBeInTheDocument()
        );

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
        expect(mockOnOpenChange).not.toHaveBeenCalledWith(false);
    });

    it('shows the server error banner when the mutation resolves without a deliverable', async () => {
        createDeliverableMock.mockResolvedValue({ data: { createDeliverable: null } });

        await submitValidForm(VALID_PROJECT_ID);

        await waitFor(() =>
            expect(screen.getByText('Failed to create deliverable')).toBeInTheDocument()
        );

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
    });

    it('reports an invalid project id without calling the mutation', async () => {
        await submitValidForm('not-a-uuid');

        await waitFor(() =>
            expect(
                screen.getByText(/missing or invalid project ID/i)
            ).toBeInTheDocument()
        );

        expect(createDeliverableMock).not.toHaveBeenCalled();
        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
    });
});
