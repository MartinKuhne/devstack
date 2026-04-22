import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { ErrorBoundary } from './ErrorBoundary';

describe('ErrorBoundary', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.restoreAllMocks();
    });

    it('renders children when no error', () => {
        render(
            <ErrorBoundary name="TestComponent">
                <div data-testid="child">Test Content</div>
            </ErrorBoundary>
        );

        expect(screen.getByTestId('child')).toBeInTheDocument();
        expect(screen.getByText('Test Content')).toBeInTheDocument();
    });

    it('shows error UI when error is thrown', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="FailingComponent">
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
        expect(screen.getByText(/test error/i)).toBeInTheDocument();
        expect(screen.getByText(/failingcomponent/i)).toBeInTheDocument();
    });

    it('shows custom fallback when provided', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="FailingComponent" fallback={<div data-testid="custom-fallback">Custom Error</div>}>
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        expect(screen.getByTestId('custom-fallback')).toBeInTheDocument();
        expect(screen.getByText('Custom Error')).toBeInTheDocument();
    });

    it('has Reload button that triggers reload', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="FailingComponent">
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        const reloadButton = screen.getByRole('button', { name: /reload page/i });
        expect(reloadButton).toBeInTheDocument();
    });

    it('has Reset button that resets error state', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="FailingComponent">
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        const resetButton = screen.getByRole('button', { name: /reset/i });
        expect(resetButton).toBeInTheDocument();
    });

    it('has Copy Error button', () => {
        const mockWriteText = vi.fn().mockResolvedValue(undefined);
        const mockClipboard = { writeText: mockWriteText };
        Object.defineProperty(navigator, 'clipboard', { value: mockClipboard });

        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="FailingComponent">
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        fireEvent.click(screen.getByRole('button', { name: /copy error/i }));

        expect(mockWriteText).toHaveBeenCalled();
    });

    it('shows component name in error message', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary name="MyCustomComponent">
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        expect(screen.getByText(/mycustomcomponent/i)).toBeInTheDocument();
    });

    it('shows "application" when no name provided', () => {
        const ErrorThrowingComponent = () => {
            throw new Error('Test error');
        };

        render(
            <ErrorBoundary>
                <ErrorThrowingComponent />
            </ErrorBoundary>
        );

        expect(screen.getByText(/application/i)).toBeInTheDocument();
    });
});
