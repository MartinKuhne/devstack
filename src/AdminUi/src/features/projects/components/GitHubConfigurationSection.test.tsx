import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { GitHubConfigurationSection } from './GitHubConfigurationSection';

describe('GitHubConfigurationSection', () => {
    const mockProjectWithRepo = {
        id: 'proj-1',
        name: 'Test Project',
        description: 'A test project',
        repository: 'https://github.com/test/repo',
    };

    const mockProjectWithoutRepo = {
        id: 'proj-1',
        name: 'Test Project',
        description: 'A test project',
        repository: null,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows repository URL when configured', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        expect(screen.getByDisplayValue('https://github.com/test/repo')).toBeInTheDocument();
        expect(screen.getByRole('link', { name: /open/i })).toBeInTheDocument();
    });

    it('shows "No GitHub repository configured" when repo is null', () => {
        render(<GitHubConfigurationSection project={mockProjectWithoutRepo} />);

        expect(screen.getByText(/no github repository configured/i)).toBeInTheDocument();
    });

    it('opens repository link in new tab', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const link = screen.getByRole('link', { name: /open/i });
        expect(link).toHaveAttribute('href', 'https://github.com/test/repo');
        expect(link).toHaveAttribute('target', '_blank');
        expect(link).toHaveAttribute('rel', 'noopener noreferrer');
    });

    it('shows token input with password type by default', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const tokenInput = screen.getByLabelText(/github token/i);
        expect(tokenInput).toHaveAttribute('type', 'password');
    });

    it('toggles token visibility when eye icon is clicked', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const toggleButton = screen.getByRole('button', { name: '' });
        expect(screen.getByLabelText(/github token/i)).toHaveAttribute('type', 'password');

        fireEvent.click(toggleButton);
        expect(screen.getByLabelText(/github token/i)).toHaveAttribute('type', 'text');

        fireEvent.click(toggleButton);
        expect(screen.getByLabelText(/github token/i)).toHaveAttribute('type', 'password');
    });

    it('clears token input when Clear button is clicked', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const tokenInput = screen.getByLabelText(/github token/i);
        fireEvent.change(tokenInput, { target: { value: 'my-token' } });
        expect(tokenInput).toHaveValue('my-token');

        fireEvent.click(screen.getByRole('button', { name: /clear/i }));
        expect(tokenInput).toHaveValue('');
    });

    it('Save button is disabled when token is empty', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const saveButton = screen.getByRole('button', { name: /save/i });
        expect(saveButton).toBeDisabled();
    });

    it('Save button is enabled when token is entered', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const tokenInput = screen.getByLabelText(/github token/i);
        const saveButton = screen.getByRole('button', { name: /save/i });

        fireEvent.change(tokenInput, { target: { value: 'my-token' } });
        expect(saveButton).toBeEnabled();
    });

    it('Clear button is disabled when token is empty', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const clearButton = screen.getByRole('button', { name: /clear/i });
        expect(clearButton).toBeDisabled();
    });

    it('Clear button is enabled when token is entered', () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const tokenInput = screen.getByLabelText(/github token/i);
        const clearButton = screen.getByRole('button', { name: /clear/i });

        fireEvent.change(tokenInput, { target: { value: 'my-token' } });
        expect(clearButton).toBeEnabled();
    });

    it('disables token input during updating', async () => {
        render(<GitHubConfigurationSection project={mockProjectWithRepo} />);

        const tokenInput = screen.getByLabelText(/github token/i);
        fireEvent.change(tokenInput, { target: { value: 'my-token' } });

        const saveButton = screen.getByRole('button', { name: /save/i });
        fireEvent.click(saveButton);

        await new Promise((r) => setTimeout(r, 100));
        expect(tokenInput).toBeDisabled();
    });
});
