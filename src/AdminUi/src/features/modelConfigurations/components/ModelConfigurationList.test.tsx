import { render, screen, waitFor } from '@testing-library/react';
import { ModelConfigurationList } from './ModelConfigurationList';
import { vi } from 'vitest';

describe('ModelConfigurationList', () => {
  const mockProjectId = 'test-project-id';
  const mockOnAddModel = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render loading state when loading is true', () => {
    render(
      <ModelConfigurationList
        projectId={mockProjectId}
        onAddModel={mockOnAddModel}
      />
    );

    // Mock the hook to return loading state
    vi.mocked(useModelConfigurations).mockReturnValue({
      modelConfigurations: [],
      loading: true,
      error: null,
    });

    expect(screen.getByText(/model configurations/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add model/i })).toBeInTheDocument();
    // Should show skeleton placeholders
    expect(screen.getAllByRole('img', { hidden: true })).toHaveLength(3); // 3 skeleton cards
  });

  it('should render error state when error is present', () => {
    render(
      <ModelConfigurationList
        projectId={mockProjectId}
        onAddModel={mockOnAddModel}
      />
    );

    // Mock the hook to return error state
    vi.mocked(useModelConfigurations).mockReturnValue({
      modelConfigurations: [],
      loading: false,
      error: new Error('Failed to load'),
    });

    expect(screen.getByText(/error loading model configurations/i)).toBeInTheDocument();
  });

  it('should render empty state when no configurations exist', () => {
    render(
      <ModelConfigurationList
        projectId={mockProjectId}
        onAddModel={mockOnAddModel}
      />
    );

    // Mock the hook to return empty data
    vi.mocked(useModelConfigurations).mockReturnValue({
      modelConfigurations: [],
      loading: false,
      error: null,
    });

    expect(screen.getByText(/model configurations/i)).toBeInTheDocument();
    expect(screen.getByText(/no model configurations yet/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add model/i })).toBeInTheDocument();
  });

  it('should render model configurations when data exists', () => {
    render(
      <ModelConfigurationList
        projectId={mockProjectId}
        onAddModel={mockOnAddModel}
      />
    );

    // Mock the hook to return data
    vi.mocked(useModelConfigurations).mockReturnValue({
      modelConfigurations: [
        {
          id: '1',
          model: 'test-model-1',
          modelAlias: 'Test Alias 1',
          url: 'https://test1.example.com',
          maxComplexity: 5,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
        {
          id: '2',
          model: 'test-model-2',
          modelAlias: null,
          url: 'https://test2.example.com',
          maxComplexity: 8,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
      ],
      loading: false,
      error: null,
    });

    expect(screen.getByText(/test model 1/i)).toBeInTheDocument();
    expect(screen.getByText(/test model 2/i)).toBeInTheDocument();
    expect(screen.getByText(/test alias 1/i)).toBeInTheDocument();
    expect(screen.getByText(/https:\/\/test1.example.com/i)).toBeInTheDocument();
    expect(screen.getByText(/https:\/\/test2.example.com/i)).toBeInTheDocument();
    expect(screen.getByText(/5/i)).toBeInTheDocument();
    expect(screen.getByText(/8/i)).toBeInTheDocument();
    
    // Should show 2 model cards
    expect(screen.getAllByRole('heading', { level: 3 })).toHaveLength(2);
  });

  it('should call onAddModel when Add Model button is clicked', () => {
    render(
      <ModelConfigurationList
        projectId={mockProjectId}
        onAddModel={mockOnAddModel}
      />
    );

    // Mock the hook to return empty data
    vi.mocked(useModelConfigurations).mockReturnValue({
      modelConfigurations: [],
      loading: false,
      error: null,
    });

    fireEvent.click(screen.getByRole('button', { name: /add model/i }));
    expect(mockOnAddModel).toHaveBeenCalledTimes(1);
  });
});

// Mock the hook
vi.mock('@/features/modelConfigurations/hooks/useModelConfigurations');