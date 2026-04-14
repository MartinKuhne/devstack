import { Badge } from '@/components/ui/badge';
import type { DefectSeverity } from '@/generated/graphql';

export const SEVERITY_COLORS: Record<DefectSeverity, string> = {
    Critical: 'bg-red-600',
    High: 'bg-red-500',
    Medium: 'bg-yellow-500',
    Low: 'bg-green-500',
};

interface SeverityBadgeProps {
    severity: DefectSeverity;
    className?: string;
}

export function SeverityBadge({ severity, className }: SeverityBadgeProps) {
    return (
        <Badge className={`${SEVERITY_COLORS[severity] || 'bg-gray-500'} ${className ?? ''}`.trim()}>
            {severity}
        </Badge>
    );
}
