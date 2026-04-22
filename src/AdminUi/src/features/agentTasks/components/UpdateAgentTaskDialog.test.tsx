import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

vi.mock('@/generated/graphql', () => ({
    useUpdateAgentTaskMutation: vi.fn(),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useUpdateAgentTaskMutation: m.useUpdateAgentTaskMutation as ReturnType<typeof vi.fn>,
    };
};

describe('UpdateAgentTaskDialog', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();

    const mockAgentTask = {
        id: 'task-1',
        title: 'Test Task',
        description: 'A test task description',
        complexityRating: 5,
        result: 'Task result',
        status: 'READY',
        deliverableId: 'del-123',
    };

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedHooks();
        hooks.useUpdateAgentTaskMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('renders form with correct labels', async () => {
        const { UpdateAgentTaskDialog } = await import('./UpdateAgentTaskDialog');
        render(
            <UpdateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                agentTask={mockAgentTask}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/complexity rating/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    });

    it('pre-populates form with agent task data', async () => {
        const { UpdateAgentTaskDialog } = await import('./UpdateAgentTaskDialog');
        render(
            <UpdateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                agentTask={mockAgentTask}
                onSuccess={mockOnSuccess}
            />
        );

        const titleInput = screen.getByLabelText(/title/i);
        expect(titleInput).toHaveValue('Test Task');

        const complexityInput = screen.getByLabelText(/complexity rating/i) as HTMLInputElement;
        expect(complexityInput.value).toBe('5');
    });

    it('has correct dialog title', async () => {
        const { UpdateAgentTaskDialog } = await import('./UpdateAgentTaskDialog');
        render(
            <UpdateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                agentTask={mockAgentTask}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/edit agent task/i)).toBeInTheDocument();
    });

    it('calls onOpenChange when cancel button is clicked', async () => {
        const { UpdateAgentTaskDialog } = await import('./UpdateAgentTaskDialog');
        render(
            <UpdateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                agentTask={mockAgentTask}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('returns null when agentTask is null', async () => {
        const { UpdateAgentTaskDialog } = await import('./UpdateAgentTaskDialog');
        const { container } = render(
            <UpdateAgentTaskDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                agentTask={null}
                onSuccess={mockOnSuccess}
            />
        );

        expect(container.firstChild).toBeNull();
    });
});
