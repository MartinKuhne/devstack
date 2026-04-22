import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

vi.mock('@/generated/graphql', () => ({
    useUpdateDeliverableMutation: vi.fn(),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useUpdateDeliverableMutation: m.useUpdateDeliverableMutation as ReturnType<typeof vi.fn>,
    };
};

describe('EditDeliverableDialog', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();

    const mockDeliverable = {
        id: 'del-1',
        title: 'Test Deliverable',
        description: 'A test description',
        acceptanceCriteria: 'Acceptance criteria',
        executionPlan: 'Execution plan',
        securityImpact: 'No impact',
        performanceImpact: 'Minimal impact',
        testPlan: 'Test plan',
        deploymentPlan: 'Deploy to staging',
        blocking: null,
    };

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedHooks();
        hooks.useUpdateDeliverableMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('renders form with correct labels', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={mockDeliverable}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/acceptance criteria/i)).toBeInTheDocument();
    });

    it('pre-populates form with deliverable data', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={mockDeliverable}
                onSuccess={mockOnSuccess}
            />
        );

        const titleInput = screen.getByLabelText(/title/i);
        expect(titleInput).toHaveValue('Test Deliverable');
    });

    it('has correct dialog title', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={mockDeliverable}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/edit deliverable/i)).toBeInTheDocument();
    });

    it('calls onOpenChange when cancel button is clicked', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={mockDeliverable}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('returns null when deliverable is null', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        const { container } = render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={null}
                onSuccess={mockOnSuccess}
            />
        );

        expect(container.firstChild).toBeNull();
    });

    it('renders all textareas for deliverable fields', async () => {
        const { EditDeliverableDialog } = await import('./EditDeliverableDialog');
        render(
            <EditDeliverableDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                deliverable={mockDeliverable}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/acceptance criteria/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/execution plan/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/security impact/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/performance impact/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/test plan/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/deployment plan/i)).toBeInTheDocument();
    });
});
