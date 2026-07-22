import { render, screen, fireEvent } from '@testing-library/react';
import { CreateLargeLanguageModelDialog } from './CreateLargeLanguageModelDialog';
import { vi, beforeEach } from 'vitest';

vi.mock('@/generated/graphql', () => ({
    useCreateLargeLanguageModelMutation: vi.fn(),
    useUpdateLargeLanguageModelMutation: vi.fn(),
}));

const getMockedGraphqlHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useCreateLargeLanguageModelMutation: m.useCreateLargeLanguageModelMutation as ReturnType<typeof vi.fn>,
        useUpdateLargeLanguageModelMutation: m.useUpdateLargeLanguageModelMutation as ReturnType<typeof vi.fn>,
    };
};

describe('CreateLargeLanguageModelDialog', () => {
    const mockOnSuccess = vi.fn();
    const mockOnOpenChange = vi.fn();

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedGraphqlHooks();
        hooks.useCreateLargeLanguageModelMutation.mockReturnValue([vi.fn(), { loading: false }]);
        hooks.useUpdateLargeLanguageModelMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('should render form with correct labels', () => {
        render(
            <CreateLargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/endpoint url/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/model name/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/alias \(optional\)/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/api key/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/max concurrency/i)).toBeInTheDocument();
    });

    it('should have correct dialog title', () => {
        render(
            <CreateLargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/add large language model/i)).toBeInTheDocument();
    });

    it('should call onOpenChange when cancel button is clicked', () => {
        render(
            <CreateLargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });
});
