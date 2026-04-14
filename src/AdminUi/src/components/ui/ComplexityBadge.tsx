import { Badge } from '@/components/ui/badge';

const COMPLEXITY_COLORS: Record<string, string> = {
    Simple: 'bg-green-500',
    Moderate: 'bg-yellow-500',
    Complex: 'bg-orange-500',
    Major: 'bg-red-500',
};

interface ComplexityBadgeProps {
    complexity: string | null | undefined;
}

export function ComplexityBadge({ complexity }: ComplexityBadgeProps) {
    if (!complexity) {
        return <Badge className="bg-gray-500">Unknown</Badge>;
    }

    return (
        <Badge className={COMPLEXITY_COLORS[complexity] || 'bg-gray-500'}>
            {complexity}
        </Badge>
    );
}
