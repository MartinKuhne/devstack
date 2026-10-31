import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';

vi.mock('@/generated/graphql', () => ({
    useCreateAgentTaskMutation: vi.fn(),
    useGetAgentTasksQuery: vi.fn().mockReturnValue({ data: undefined, loading: false, error: undefined, refetch: vi.fn() }),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useCreateAgentTaskMutation: m.useCreateAgentTaskMutation as ReturnType<typeof vi.fn>,
    };
};

describe('CreateAgentTaskDialog', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();
    const mockDeliverableId = 'del-123';
    const mockProjectId = 'proj-123';

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedHooks();
        hooks.useCreateAgentTaskMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('renders form with correct labels', async () => {
        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/deliverable id/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/complexity rating/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    });

    it('has correct dialog title', async () => {
        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/create new agent task/i)).toBeInTheDocument();
    });

    it('sets deliverableId as disabled input value', async () => {
        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        const deliverableInput = screen.getByLabelText(/deliverable id/i);
        expect(deliverableInput).toHaveValue(mockDeliverableId);
        expect(deliverableInput).toBeDisabled();
    });

    it('calls onOpenChange when cancel button is clicked', async () => {
        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('has complexity rating input with correct attributes', async () => {
        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        const complexityInput = screen.getByLabelText(/complexity rating/i) as HTMLInputElement;
        expect(complexityInput.type).toBe('number');
        expect(complexityInput).toHaveAttribute('min', '1');
        expect(complexityInput).toHaveAttribute('max', '10');
    });

    it('sends the projectId prop in the CreateAgentTaskInput', async () => {
        const createTaskMock = vi
            .fn()
            .mockResolvedValue({ data: { createAgentTask: { id: 'task-1' } } });
        const hooks = await getMockedHooks();
        hooks.useCreateAgentTaskMutation.mockReturnValue([createTaskMock, { loading: false }]);

        const { CreateAgentTaskDialog } = await import('./CreateAgentTaskDialog');
        render(
            <CreateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverableId={mockDeliverableId}
                projectId={mockProjectId}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.change(screen.getByLabelText(/title \*/i), {
            target: { value: 'Implement the feature' },
        });

        const submitButton = screen.getByRole('button', { name: /create agent task/i });
        await waitFor(() => expect(submitButton).toBeEnabled());
        fireEvent.click(submitButton);

        await waitFor(() =>
            expect(createTaskMock).toHaveBeenCalledWith({
                variables: {
                    input: {
                        projectId: mockProjectId,
                        deliverableId: mockDeliverableId,
                        title: 'Implement the feature',
                        description: '',
                        complexityRating: 5,
                        dependsOnAgentTaskId: null,
                    },
                },
            })
        );
        expect(mockOnSuccess).toHaveBeenCalledWith('task-1');
    });
});
