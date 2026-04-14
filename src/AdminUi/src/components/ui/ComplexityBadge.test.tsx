import { render, screen } from '@testing-library/react';
import { ComplexityBadge } from './ComplexityBadge';

describe('ComplexityBadge', () => {
    it('renders Simple complexity with green color', () => {
        render(<ComplexityBadge complexity="Simple" />);
        const badge = screen.getByText('Simple');
        expect(badge).toHaveClass('bg-green-500');
    });

    it('renders Moderate complexity with yellow color', () => {
        render(<ComplexityBadge complexity="Moderate" />);
        const badge = screen.getByText('Moderate');
        expect(badge).toHaveClass('bg-yellow-500');
    });

    it('renders Complex complexity with orange color', () => {
        render(<ComplexityBadge complexity="Complex" />);
        const badge = screen.getByText('Complex');
        expect(badge).toHaveClass('bg-orange-500');
    });

    it('renders Major complexity with red color', () => {
        render(<ComplexityBadge complexity="Major" />);
        const badge = screen.getByText('Major');
        expect(badge).toHaveClass('bg-red-500');
    });

    it('renders Unknown for null complexity', () => {
        render(<ComplexityBadge complexity={null} />);
        const badge = screen.getByText('Unknown');
        expect(badge).toHaveClass('bg-gray-500');
    });

    it('renders Unknown for undefined complexity', () => {
        render(<ComplexityBadge complexity={undefined} />);
        const badge = screen.getByText('Unknown');
        expect(badge).toHaveClass('bg-gray-500');
    });

    it('renders Invalid complexity with gray fallback color', () => {
        render(<ComplexityBadge complexity="Invalid" />);
        const badge = screen.getByText('Invalid');
        expect(badge).toHaveClass('bg-gray-500');
    });
});
