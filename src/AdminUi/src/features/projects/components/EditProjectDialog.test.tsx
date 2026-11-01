import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { GraphQLError } from 'graphql';
import { CombinedGraphQLErrors } from '@apollo/client/errors';

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

    const mockProject = {
        id: 'proj-1',
        name: 'Test Project',
        description: 'A test project',
        repository: 'https://github.com/test/repo',
    };

    let updateProjectMock: ReturnType<typeof vi.fn>;

    /**
     * Renders the dialog for `project`, fills the name field and submits the form.
     * The dialog reports failures through its inline server-error banner; it has no onError prop.
     */
    async function submitValidForm(project: typeof mockProject | null) {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={project}
                onSuccess={mockOnSuccess}
            />
        );

        if (!project) return;

        fireEvent.change(screen.getByLabelText(/name \*/i), {
            target: { value: '[DeleteAfterTest] Renamed Project' },
        });

        const submitButton = screen.getByRole('button', { name: /save changes/i });
        await waitFor(() => expect(submitButton).toBeEnabled());
        fireEvent.click(submitButton);
    }

    beforeEach(async () => {
        vi.clearAllMocks();
        updateProjectMock = vi.fn();
        const hooks = await getMockedHooks();
        hooks.useUpdateProjectMutation.mockReturnValue([updateProjectMock, { loading: false }]);
    });

    it('renders form with correct labels and inputs', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={mockProject}
                onSuccess={mockOnSuccess}
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
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('renders nothing when project is null', async () => {
        const { EditProjectDialog } = await import('./EditProjectDialog');
        render(
            <EditProjectDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                project={null}
                onSuccess={mockOnSuccess}
            />
        );

        // Radix portals the dialog into document.body, so asserting on the render
        // container would pass even if a dialog had been rendered.
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
        expect(updateProjectMock).not.toHaveBeenCalled();
    });

    it('sends the mutation with the id of the edited project', async () => {
        updateProjectMock.mockResolvedValue({ data: { updateProject: { id: 'proj-1' } } });

        await submitValidForm(mockProject);

        await waitFor(() =>
            expect(updateProjectMock).toHaveBeenCalledWith({
                variables: {
                    input: {
                        id: 'proj-1',
                        name: '[DeleteAfterTest] Renamed Project',
                        description: 'A test project',
                        repository: 'https://github.com/test/repo',
                    },
                },
            })
        );
        expect(mockOnSuccess).toHaveBeenCalledTimes(1);
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('shows the server error banner and keeps the dialog open on a rejected mutation', async () => {
        updateProjectMock.mockRejectedValue(
            new CombinedGraphQLErrors({
                data: { updateProject: null },
                errors: [new GraphQLError('A project with this repository already exists.')],
            })
        );

        await submitValidForm(mockProject);

        await waitFor(() =>
            expect(
                screen.getByText('A project with this repository already exists.')
            ).toBeInTheDocument()
        );

        // the dialog must stay open so the user can fix the input
        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
        expect(mockOnOpenChange).not.toHaveBeenCalledWith(false);
    });

    it('shows the server error banner when the mutation resolves without a project', async () => {
        updateProjectMock.mockResolvedValue({ data: { updateProject: null } });

        await submitValidForm(mockProject);

        await waitFor(() => expect(screen.getByText('Failed to update project')).toBeInTheDocument());

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
        expect(mockOnOpenChange).not.toHaveBeenCalledWith(false);
    });

    it('surfaces a network failure in the banner instead of swallowing it', async () => {
        updateProjectMock.mockRejectedValue(new Error('Network request failed'));

        await submitValidForm(mockProject);

        await waitFor(() => expect(screen.getByText('Network request failed')).toBeInTheDocument());

        expect(screen.getByRole('dialog')).toBeInTheDocument();
        expect(mockOnSuccess).not.toHaveBeenCalled();
    });
});
