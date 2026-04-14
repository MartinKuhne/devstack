import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { SeverityBadge, SEVERITY_COLORS } from './SeverityBadge';
import type { DefectSeverity } from '@/generated/graphql';

describe('SeverityBadge', () => {
    describe('SEVERITY_COLORS', () => {
        it('should have correct color for Critical severity', () => {
            expect(SEVERITY_COLORS.Critical).toBe('bg-red-600');
        });

        it('should have correct color for High severity', () => {
            expect(SEVERITY_COLORS.High).toBe('bg-red-500');
        });

        it('should have correct color for Medium severity', () => {
            expect(SEVERITY_COLORS.Medium).toBe('bg-yellow-500');
        });

        it('should have correct color for Low severity', () => {
            expect(SEVERITY_COLORS.Low).toBe('bg-green-500');
        });

        it('should have all four severity levels', () => {
            const severities: DefectSeverity[] = ['Critical', 'High', 'Medium', 'Low'];
            severities.forEach(severity => {
                expect(SEVERITY_COLORS[severity]).toBeDefined();
            });
        });
    });

    describe('SeverityBadge component', () => {
        it('should render the severity text', () => {
            render(<SeverityBadge severity="Critical" />);
            expect(screen.getByText('Critical')).toBeInTheDocument();
        });

        it('should apply the correct color class for Critical', () => {
            render(<SeverityBadge severity="Critical" />);
            const badge = screen.getByText('Critical');
            expect(badge).toHaveClass('bg-red-600');
        });

        it('should apply the correct color class for High', () => {
            render(<SeverityBadge severity="High" />);
            const badge = screen.getByText('High');
            expect(badge).toHaveClass('bg-red-500');
        });

        it('should apply the correct color class for Medium', () => {
            render(<SeverityBadge severity="Medium" />);
            const badge = screen.getByText('Medium');
            expect(badge).toHaveClass('bg-yellow-500');
        });

        it('should apply the correct color class for Low', () => {
            render(<SeverityBadge severity="Low" />);
            const badge = screen.getByText('Low');
            expect(badge).toHaveClass('bg-green-500');
        });

        it('should accept custom className prop', () => {
            render(<SeverityBadge severity="Medium" className="custom-class" />);
            const badge = screen.getByText('Medium');
            expect(badge).toHaveClass('custom-class');
        });
    });
});
