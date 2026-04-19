import { render, screen, fireEvent } from '@testing-library/react';
import { LargeLanguageModelDialog } from './LargeLanguageModelDialog';
import { vi, beforeEach } from 'vitest';

// Mock the GraphQL mutation hooks
vi.mock('@/generated/graphql', () => ({
    useCreateModelConfigurationMutation: vi.fn(),
    useUpdateModelConfigurationMutation: vi.fn(),
    useDeleteModelConfigurationMutation: vi.fn(),
}));

const getMockedGraphqlHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useCreateModelConfigurationMutation: m.useCreateModelConfigurationMutation as ReturnType<typeof vi.fn>,
        useUpdateModelConfigurationMutation: m.useUpdateModelConfigurationMutation as ReturnType<typeof vi.fn>,
        useDeleteModelConfigurationMutation: m.useDeleteModelConfigurationMutation as ReturnType<typeof vi.fn>,
    };
};

describe('LargeLanguageModelDialog', () => {
    const mockOnSuccess = vi.fn();
    const mockOnOpenChange = vi.fn();

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedGraphqlHooks();
        hooks.useCreateModelConfigurationMutation.mockReturnValue([vi.fn(), { loading: false }]);
        hooks.useUpdateModelConfigurationMutation.mockReturnValue([vi.fn(), { loading: false }]);
        hooks.useDeleteModelConfigurationMutation.mockReturnValue([vi.fn(), { loading: false }]);
    });

    it('should render form with correct labels', () => {
        render(
            <LargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByLabelText(/endpoint url/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/model name/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/alias \(optional\)/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/api key/i)).toBeInTheDocument();
    });

    it('should have correct dialog title', () => {
        render(
            <LargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        expect(screen.getByText(/add large language model/i)).toBeInTheDocument();
    });

    it('should call onOpenChange when cancel button is clicked', () => {
        render(
            <LargeLanguageModelDialog
                open={true}
                onOpenChange={mockOnOpenChange}
                onSuccess={mockOnSuccess}
            />
        );

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
        expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });
});
