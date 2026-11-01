import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ErrorState } from './ErrorState';

describe('ErrorState', () => {
    it('renders an alert region with the message', () => {
        render(<ErrorState message="Unable to load projects." />);

        const alert = screen.getByRole('alert');
        expect(alert).toBeInTheDocument();
        expect(alert).toHaveTextContent('Unable to load projects.');
    });

    it('uses the default title when none is provided', () => {
        render(<ErrorState message="Boom" />);

        expect(screen.getByRole('heading', { name: 'Error', level: 3 })).toBeInTheDocument();
    });

    it('renders a custom title and the technical detail', () => {
        render(
            <ErrorState
                title="Deliverable could not be saved"
                message="Please try again."
                detail="Deliverable with ID 42 not found."
            />
        );

        expect(
            screen.getByRole('heading', { name: 'Deliverable could not be saved', level: 3 })
        ).toBeInTheDocument();
        expect(screen.getByText('Please try again.')).toBeInTheDocument();
        expect(screen.getByText('Deliverable with ID 42 not found.')).toBeInTheDocument();
    });

    it('omits the detail paragraph when no detail is provided', () => {
        render(<ErrorState message="Boom" />);

        expect(screen.queryByText('Boom')).toBeInTheDocument();
        expect(screen.queryByText(/undefined/)).not.toBeInTheDocument();
    });

    it('calls the retry handler when the retry button is clicked', () => {
        const onRetry = vi.fn();
        render(<ErrorState message="Boom" onRetry={onRetry} />);

        const button = screen.getByRole('button', { name: 'Retry' });
        expect(button).toBeInTheDocument();

        fireEvent.click(button);
        expect(onRetry).toHaveBeenCalledTimes(1);

        fireEvent.click(button);
        expect(onRetry).toHaveBeenCalledTimes(2);
    });

    it('supports a custom retry label', () => {
        const onRetry = vi.fn();
        render(<ErrorState message="Boom" onRetry={onRetry} retryLabel="Reload projects" />);

        expect(screen.getByRole('button', { name: 'Reload projects' })).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Retry' })).not.toBeInTheDocument();

        fireEvent.click(screen.getByRole('button', { name: 'Reload projects' }));
        expect(onRetry).toHaveBeenCalledTimes(1);
    });

    it('renders no retry button when no handler is provided', () => {
        render(<ErrorState message="Boom" />);

        expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });
});
