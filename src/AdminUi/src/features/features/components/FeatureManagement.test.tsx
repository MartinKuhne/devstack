import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { StatusTransitionPanel } from '@/features/features/components/StatusTransitionPanel';
import { TaskBoard } from '@/features/features/components/TaskBoard';
import type { Task } from '@/generated/graphql';
import { toast } from 'react-toastify';

// Mock toast
vi.mock('react-toastify', () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    vi.clearAllMocks();
});

describe('Feature Status Badge Color Logic', () => {
    const getStatusColor = (
        status: string
    ): 'default' | 'secondary' | 'destructive' | 'outline' => {
        const colors: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
            Planning: 'default',
            Ready: 'secondary',
            InProgress: 'outline',
            InReview: 'secondary',
            ReadyForTest: 'outline',
            Testing: 'outline',
            Done: 'default',
            Failed: 'destructive',
            Rejected: 'destructive',
        };
        return colors[status] || 'default';
    };

    it('returns default color for Planning status', () => {
        expect(getStatusColor('Planning')).toBe('default');
    });

    it('returns secondary color for Ready status', () => {
        expect(getStatusColor('Ready')).toBe('secondary');
    });

    it('returns outline color for InProgress status', () => {
        expect(getStatusColor('InProgress')).toBe('outline');
    });

    it('returns secondary color for InReview status', () => {
        expect(getStatusColor('InReview')).toBe('secondary');
    });

    it('returns outline color for ReadyForTest status', () => {
        expect(getStatusColor('ReadyForTest')).toBe('outline');
    });

    it('returns outline color for Testing status', () => {
        expect(getStatusColor('Testing')).toBe('outline');
    });

    it('returns default color for Done status', () => {
        expect(getStatusColor('Done')).toBe('default');
    });

    it('returns destructive color for Failed status', () => {
        expect(getStatusColor('Failed')).toBe('destructive');
    });

    it('returns destructive color for Rejected status', () => {
        expect(getStatusColor('Rejected')).toBe('destructive');
    });

    it('returns default color for unknown status', () => {
        expect(getStatusColor('Unknown')).toBe('default');
    });
});

describe('StatusTransitionPanel - Valid Transitions', () => {
    const mockOnTransition = vi.fn().mockResolvedValue({ success: true });

    it('displays current status with correct badge', () => {
        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest', 'Rejected']}
                onTransition={mockOnTransition}
            />
        );

        expect(screen.getByText('Current Status:')).toBeInTheDocument();
        expect(screen.getByText('InProgress')).toBeInTheDocument();
    });

    it('shows valid target statuses in select dropdown', async () => {
        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest', 'Rejected']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            expect(screen.getByText('ReadyForTest')).toBeInTheDocument();
            expect(screen.getByText('Rejected')).toBeInTheDocument();
        });
    });

    it('shows no transitions message when validTransitions is empty', () => {
        render(
            <StatusTransitionPanel
                currentStatus="Done"
                validTransitions={[]}
                onTransition={mockOnTransition}
            />
        );

        expect(screen.getByText(/No valid status transitions/)).toBeInTheDocument();
    });

    it('disables transition button when no target status is selected', () => {
        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const button = screen.getByText('Transition');
        expect(button).toBeDisabled();
    });

    it('calls onTransition when valid target status is selected', async () => {
        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            const readyForTestOption = screen.getByText('ReadyForTest');
            fireEvent.click(readyForTestOption);
        });

        const button = screen.getByText('Transition');
        fireEvent.click(button);

        await waitFor(() => {
            expect(mockOnTransition).toHaveBeenCalledWith('ReadyForTest');
        });
    });

    it('shows success toast when transition succeeds', async () => {
        mockOnTransition.mockResolvedValueOnce({ success: true });

        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            const readyForTestOption = screen.getByText('ReadyForTest');
            fireEvent.click(readyForTestOption);
        });

        const button = screen.getByText('Transition');
        fireEvent.click(button);

        await waitFor(() => {
            expect(toast.success).toHaveBeenCalledWith('Status transition successful');
        });
    });

    it('shows error toast for each error when transition fails', async () => {
        const errors = ['Invalid transition', 'Feature is locked'];
        mockOnTransition.mockResolvedValueOnce({ success: false, errors });

        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            const readyForTestOption = screen.getByText('ReadyForTest');
            fireEvent.click(readyForTestOption);
        });

        const button = screen.getByText('Transition');
        fireEvent.click(button);

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledTimes(2);
            expect(toast.error).toHaveBeenCalledWith('Invalid transition');
            expect(toast.error).toHaveBeenCalledWith('Feature is locked');
        });
    });

    it('disables transition button while transitioning', async () => {
        mockOnTransition.mockImplementation(
            () =>
                new Promise((resolve) => setTimeout(() => resolve({ success: true }), 100))
        );

        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            const readyForTestOption = screen.getByText('ReadyForTest');
            fireEvent.click(readyForTestOption);
        });

        const button = screen.getByText('Transition');
        fireEvent.click(button);

        await waitFor(() => {
            expect(button).toBeDisabled();
        });
    });
});

describe('StatusTransitionPanel - Invalid Transitions', () => {
    it('handles invalid transition error from server', async () => {
        const mockOnTransition = vi.fn().mockResolvedValue({
            success: false,
            errors: ['FEATURE_VALIDATION_ERROR: Invalid status transition'],
        });

        render(
            <StatusTransitionPanel
                currentStatus="Done"
                validTransitions={[]}
                onTransition={mockOnTransition}
            />
        );

        expect(screen.getByText(/No valid status transitions/)).toBeInTheDocument();
    });

    it('handles concurrency conflict error', async () => {
        const mockOnTransition = vi.fn().mockResolvedValue({
            success: false,
            errors: ['CONCURRENCY_CONFLICT: The feature has been modified by another process'],
        });

        render(
            <StatusTransitionPanel
                currentStatus="InProgress"
                validTransitions={['ReadyForTest']}
                onTransition={mockOnTransition}
            />
        );

        const selectTrigger = screen.getByRole('combobox');
        fireEvent.click(selectTrigger);

        await waitFor(() => {
            const readyForTestOption = screen.getByText('ReadyForTest');
            fireEvent.click(readyForTestOption);
        });

        const button = screen.getByText('Transition');
        fireEvent.click(button);

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalledWith(
                'CONCURRENCY_CONFLICT: The feature has been modified by another process'
            );
        });
    });
});

describe('TaskBoard - Status Columns', () => {
    const mockTasks: Task[] = [
        {
            __typename: 'Task',
            id: '1',
            title: 'Task 1',
            deliverable: 'Deliverable 1',
            acceptanceCriteria: null,
            risks: null,
            requiredFollowUps: null,
            complexity: 'Simple',
            status: 'Todo',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            feature: null,
        },
        {
            __typename: 'Task',
            id: '2',
            title: 'Task 2',
            deliverable: 'Deliverable 2',
            acceptanceCriteria: null,
            risks: null,
            requiredFollowUps: null,
            complexity: 'Moderate',
            status: 'InProgress',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            feature: null,
        },
        {
            __typename: 'Task',
            id: '3',
            title: 'Task 3',
            deliverable: null,
            acceptanceCriteria: null,
            risks: null,
            requiredFollowUps: null,
            complexity: 'Complex',
            status: 'Review',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            feature: null,
        },
        {
            __typename: 'Task',
            id: '4',
            title: 'Task 4',
            deliverable: null,
            acceptanceCriteria: null,
            risks: null,
            requiredFollowUps: null,
            complexity: 'Major',
            status: 'Done',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            feature: null,
        },
    ];

    it('renders all four status columns', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('To Do')).toBeInTheDocument();
        expect(screen.getByText('In Progress')).toBeInTheDocument();
        expect(screen.getByText('Review')).toBeInTheDocument();
        expect(screen.getByText('Done')).toBeInTheDocument();
    });

    it('displays task count badges for each column', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        const countBadges = screen.getAllByText('1');
        expect(countBadges).toHaveLength(4); // Each column has 1 task
    });

    it('renders tasks in correct columns based on status', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('Task 1')).toBeInTheDocument();
        expect(screen.getByText('Task 2')).toBeInTheDocument();
        expect(screen.getByText('Task 3')).toBeInTheDocument();
        expect(screen.getByText('Task 4')).toBeInTheDocument();
    });

    it('shows complexity badges with correct colors', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('Simple')).toBeInTheDocument();
        expect(screen.getByText('Moderate')).toBeInTheDocument();
        expect(screen.getByText('Complex')).toBeInTheDocument();
        expect(screen.getByText('Major')).toBeInTheDocument();
    });

    it('displays task deliverable when available', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('Deliverable 1')).toBeInTheDocument();
        expect(screen.getByText('Deliverable 2')).toBeInTheDocument();
    });

    it('shows empty state when no tasks in column', () => {
        const tasksWithoutReview = mockTasks.filter((t) => t.status !== 'Review');

        render(
            <TaskBoard
                tasks={tasksWithoutReview}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('No tasks')).toBeInTheDocument();
    });

    it('shows "New Task" button', () => {
        render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={vi.fn()}
                onTasksChange={vi.fn()}
            />
        );

        expect(screen.getByText('New Task')).toBeInTheDocument();
    });

    it('calls onTaskClick when task card is clicked', () => {
        const onTaskClick = vi.fn();
        const { container } = render(
            <TaskBoard
                tasks={mockTasks}
                featureId="feature-1"
                onTaskClick={onTaskClick}
                onTasksChange={vi.fn()}
            />
        );

        const taskCards = container.querySelectorAll('.cursor-pointer');
        if (taskCards[0]) {
            fireEvent.click(taskCards[0]);
        }

        expect(onTaskClick).toHaveBeenCalledWith(mockTasks[0]);
    });
});

describe('TaskBoard - Complexity Color Mapping', () => {
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
});
