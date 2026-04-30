import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

vi.mock('@/generated/graphql', () => ({
    useUpdateProjectMutation: vi.fn(),
}));

const getMockedHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useUpdateProjectMutation: m.useUpdateProjectMutation as ReturnType<typeof vi.fn>,
    };
};

describe('EditProjectDialog', () => {
    const mockOnOpenChange = vi.fn();
    const mockOnSuccess = vi.fn();
    const mockOnError = vi.fn();

    const mockProject = {
        id: 'proj-1',
        name: 'Test Project',
        description: 'A test project',
        repository: 'https://github.com/test/repo',
    };

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedHooks();
        hooks.useUpdateProjectMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('renders form with correct labels and inputs', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={mockProject}
                onSuccess={mockOnSuccess}
                onError={mockOnError}
            />
        );

        expect(screen.getByLabelText(/name/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/repository/i)).toBeInTheDocument();
    });

    it('pre-populates form with project data', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={mockProject}
                onSuccess={mockOnSuccess}
                onError={mockOnError}
            />
        );

        const nameInput = screen.getByLabelText(/name/i);
        expect(nameInput).toHaveValue('Test Project');
    });

    it('calls onOpenChange when cancel button is clicked', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={mockProject}
                onSuccess={mockOnSuccess}
                onError={mockOnError}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('returns null when project is null', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        const { container } = render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={null}
                onSuccess={mockOnSuccess}
                onError={mockOnError}
            />
        );

        expect(container.firstChild).toBeNull();
    });
});
