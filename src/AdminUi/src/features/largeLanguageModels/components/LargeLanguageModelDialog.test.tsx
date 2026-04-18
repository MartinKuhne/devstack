import { render, screen, fireEvent } from '@testing-library/react';
import { LargeLanguageModelDialog } from './LargeLanguageModelDialog';
import { vi, beforeEach } from 'vitest';
import { useCreateModelConfigurationMutation } from '@/generated/graphql';

// Mock the GraphQL mutation hook
vi.mock('@/generated/graphql', () => ({
    useCreateModelConfigurationMutation: vi.fn(),
}));

describe('LargeLanguageModelDialog', () => {
    const mockOnSuccess = vi.fn();
    const mockOnOpenChange = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
        (useCreateModelConfigurationMutation as any).mockReturnValue([
            vi.fn(),
            { loading: false },
        ]);
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
