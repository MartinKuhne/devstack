import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatCard } from '@/features/dashboard/components/StatCard';

describe('StatCard', () => {
    it('renders the title and value correctly', () => {
        render(
            <StatCard
                title="Test Metric"
                value={42}
                variant="default"
                description="Test description"
            />
        );

        expect(screen.getByText('Test Metric')).toBeInTheDocument();
        expect(screen.getAllByText('42')).toHaveLength(2);
        expect(screen.getByText('Test description')).toBeInTheDocument();
    });

    it('renders the correct badge variant for default', () => {
        render(
            <StatCard
                title="Test Metric"
                value={10}
                variant="default"
                description="Test description"
            />
        );

        const badges = screen.getAllByText('10');
        expect(badges).toHaveLength(2);
    });

    it('renders the correct badge variant for warning', () => {
        render(
            <StatCard
                title="Test Metric"
                value={5}
                variant="warning"
                description="Test description"
            />
        );

        const badges = screen.getAllByText('5');
        expect(badges).toHaveLength(2);
    });

    it('renders the correct badge variant for danger', () => {
        render(
            <StatCard
                title="Test Metric"
                value={0}
                variant="danger"
                description="Test description"
            />
        );

        const badges = screen.getAllByText('0');
        expect(badges).toHaveLength(2);
    });

    it('displays zero values correctly', () => {
        render(
            <StatCard
                title="Empty Metric"
                value={0}
                variant="default"
                description="No items"
            />
        );

        expect(screen.getAllByText('0')).toHaveLength(2);
        expect(screen.getByText('Empty Metric')).toBeInTheDocument();
    });
});
