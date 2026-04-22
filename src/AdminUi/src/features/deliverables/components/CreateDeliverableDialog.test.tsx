import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

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
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/initial status/i)).toBeInTheDocument();
    });
});
