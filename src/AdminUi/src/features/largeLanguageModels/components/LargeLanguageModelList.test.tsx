import { render, screen } from '@testing-library/react';
import { LargeLanguageModelList } from './LargeLanguageModelList';
import { vi, beforeEach } from 'vitest';
import { useLargeLanguageModels } from '../hooks/useLargeLanguageModels';

// Mock the hook
vi.mock('../hooks/useLargeLanguageModels', () => ({
    useLargeLanguageModels: vi.fn().mockReturnValue({
        largeLanguageModels: [],
        loading: false,
        error: null,
    }),
}));

describe('LargeLanguageModelList', () => {
    const mockOnAddModel = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('should render empty state when no configurations exist', () => {
        render(<LargeLanguageModelList onAddModel={mockOnAddModel} />);

        expect(screen.getByText(/no large language models yet/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /add model/i })).toBeInTheDocument();
    });

    it('should call onAddModel when Add Model button is clicked', () => {
        render(<LargeLanguageModelList onAddModel={mockOnAddModel} />);

        screen.getByRole('button', { name: /add model/i }).click();
        expect(mockOnAddModel).toHaveBeenCalledTimes(1);
    });
});
