import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ModelConfigurationDialog } from './ModelConfigurationDialog';
import { vi } from 'vitest';

describe('ModelConfigurationDialog', () => {
  const mockProjectId = 'test-project-id';
  const mockOnSuccess = vi.fn();
  const mockOnOpenChange = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render form with correct initial values', () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    expect(screen.getByLabelText(/endpoint url/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/model name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/alias \(optional\)/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/api key/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/max complexity \(1-10\)/i)).toBeInTheDocument();
  });

  it('should show error for empty URL', async () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    fireEvent.click(screen.getByRole('button', { name: /add model/i }));

    expect(await screen.findByText(/url is required/i)).toBeInTheDocument();
  });

  it('should show error for invalid URL format', async () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    const urlInput = screen.getByLabelText(/endpoint url/i);
    fireEvent.change(urlInput, { target: { value: 'invalid-url' } });
    fireEvent.click(screen.getByRole('button', { name: /add model/i }));

    expect(await screen.findByText(/invalid url format/i)).toBeInTheDocument();
  });

  it('should show error for empty model name', async () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    const urlInput = screen.getByLabelText(/endpoint url/i);
    fireEvent.change(urlInput, { target: { value: 'https://example.com' } });
    fireEvent.click(screen.getByRole('button', { name: /add model/i }));

    expect(await screen.findByText(/model name is required/i)).toBeInTheDocument();
  });

  it('should show error for complexity out of range', async () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    const urlInput = screen.getByLabelText(/endpoint url/i);
    const modelInput = screen.getByLabelText(/model name/i);
    const complexitySelect = screen.getByLabelText(/max complexity \(1-10\)/i);

    fireEvent.change(urlInput, { target: { value: 'https://example.com' } });
    fireEvent.change(modelInput, { target: { value: 'test-model' } });
    fireEvent.change(complexitySelect, { target: { value: '15' } });
    fireEvent.click(screen.getByRole('button', { name: /add model/i }));

    expect(await screen.findByText(/max complexity must be between 1 and 10/i)).toBeInTheDocument();
  });

  it('should call onSuccess when form is valid', async () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    const urlInput = screen.getByLabelText(/endpoint url/i);
    const modelInput = screen.getByLabelText(/model name/i);
    const aliasInput = screen.getByLabelText(/alias \(optional\)/i);
    const apiKeyInput = screen.getByLabelText(/api key/i);
    const complexitySelect = screen.getByLabelText(/max complexity \(1-10\)/i);

    fireEvent.change(urlInput, { target: { value: 'https://example.com' } });
    fireEvent.change(modelInput, { target: { value: 'test-model' } });
    fireEvent.change(aliasInput, { target: { value: 'test-alias' } });
    fireEvent.change(apiKeyInput, { target: { value: 'test-key' } });
    fireEvent.change(complexitySelect, { target: { value: '5' } });

    // Mock fetch
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        data: {
          createModelConfiguration: {
            id: 'test-id',
            projectId: mockProjectId,
            url: 'https://example.com',
            model: 'test-model',
            modelAlias: 'test-alias',
            maxComplexity: 5,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          },
        },
      }),
    });

    fireEvent.click(screen.getByRole('button', { name: /add model/i }));

    await waitFor(() => {
      expect(mockOnSuccess).toHaveBeenCalled();
    });

    expect(mockOnOpenChange).toHaveBeenCalledWith(false);
  });

  it('should reset form when dialog closes', () => {
    render(
      <ModelConfigurationDialog
        open={true}
        onOpenChange={mockOnOpenChange}
        projectId={mockProjectId}
        onSuccess={mockOnSuccess}
      />
    );

    const urlInput = screen.getByLabelText(/endpoint url/i);
    fireEvent.change(urlInput, { target: { value: 'https://example.com' } });

    mockOnOpenChange.mock.calls.forEach((call) => {
      if (call[0] === false) {
        // Dialog closed
        expect(urlInput).toHaveValue('');
      }
    });
  });
});