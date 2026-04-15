import { Badge } from '@/components/ui/badge';
import type { Severity } from '@/generated/graphql';

export const SEVERITY_COLORS: Record<Severity, string> = {
    CRITICAL: 'bg-red-600',
    HIGH: 'bg-red-500',
    MEDIUM: 'bg-yellow-500',
    LOW: 'bg-green-500',
};

interface SeverityBadgeProps {
    severity: Severity;
    className?: string;
}

export function SeverityBadge({ severity, className }: SeverityBadgeProps) {
    return (
        <Badge className={`${SEVERITY_COLORS[severity] || 'bg-gray-500'} ${className ?? ''}`.trim()}>
            {severity}
        </Badge>
    );
}
