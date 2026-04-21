import { Button } from '@/components/ui/button';
import { FolderOpen } from 'lucide-react';

export type EmptyStateProps = {
    title?: string;
    description: string;
    action?: {
        label: string;
        onClick: () => void;
    };
    icon?: React.ComponentType<{ className?: string }>;
    className?: string;
};

export function EmptyState({
    title = 'No items found',
    description,
    action,
    icon: Icon = FolderOpen,
    className,
}: EmptyStateProps) {
    return (
        <div className={className}>
            <div className="flex flex-col items-center justify-center py-12 text-center">
                <div className="rounded-full bg-muted p-4 mb-4">
                    <Icon className="h-8 w-8 text-muted-foreground" />
                </div>
                {title && <h3 className="text-lg font-semibold mb-1">{title}</h3>}
                <p className="text-muted-foreground max-w-sm mb-4">{description}</p>
                {action && (
                    <Button onClick={action.onClick}>
                        {action.label}
                    </Button>
                )}
            </div>
        </div>
    );
}
