import { render, screen } from '@testing-library/react';
import { LargeLanguageModelList } from './LargeLanguageModelList';
import { vi, beforeEach } from 'vitest';

// Mock the hook
vi.mock('../hooks/useLargeLanguageModels', () => ({
    useLargeLanguageModels: vi.fn().mockReturnValue({
        largeLanguageModels: [],
        loading: false,
        error: null,
    }),
}));

// Mock the GraphQL mutation hooks used by LargeLanguageModelList and LargeLanguageModelDialog
vi.mock(import('@/generated/graphql'), async (importOriginal) => {
    const actual = await importOriginal();
    return {
        ...actual,
        useDeleteLargeLanguageModelMutation: vi.fn(),
        useCreateLargeLanguageModelMutation: vi.fn(),
        useUpdateLargeLanguageModelMutation: vi.fn(),
    };
});

const getMockedGraphqlHooks = async () => {
    const m = await import('@/generated/graphql');
    return {
        useDeleteLargeLanguageModelMutation: m.useDeleteLargeLanguageModelMutation as ViFnMock,
        useCreateLargeLanguageModelMutation: m.useCreateLargeLanguageModelMutation as ViFnMock,
        useUpdateLargeLanguageModelMutation: m.useUpdateLargeLanguageModelMutation as ViFnMock,
    };
};

type ViFnMock = ReturnType<typeof vi.fn>;

describe('LargeLanguageModelList', () => {
    const mockOnAddModel = vi.fn();

    beforeEach(async () => {
        vi.clearAllMocks();
        const hooks = await getMockedGraphqlHooks();
        hooks.useDeleteLargeLanguageModelMutation.mockReturnValue([vi.fn(), { loading: false }]);
        hooks.useCreateLargeLanguageModelMutation.mockReturnValue([vi.fn(), { loading: false }]);
        hooks.useUpdateLargeLanguageModelMutation.mockReturnValue([vi.fn(), { loading: false }]);
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
