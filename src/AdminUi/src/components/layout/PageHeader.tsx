import { Button, type ButtonVariant, type ButtonSize } from '@/components/ui/button';
import type { ReactNode } from 'react';

export type PageHeaderAction = {
    label: string;
    onClick: () => void;
    variant?: ButtonVariant;
    size?: ButtonSize;
    disabled?: boolean;
};

export type PageHeaderProps = {
    title: string;
    description?: string;
    actions?: PageHeaderAction[];
    actionSlot?: ReactNode;
    className?: string;
};

export function PageHeader({ title, description, actions, actionSlot, className }: PageHeaderProps) {
    return (
        <div className={className}>
            <div className="flex items-center justify-between gap-4">
                <div className="space-y-1">
                    <h2 className="text-2xl font-bold tracking-tight">{title}</h2>
                    {description && <p className="text-muted-foreground">{description}</p>}
                </div>
                <div className="flex items-center gap-2">
                    {actionSlot}
                    {actions?.map((action, index) => (
                        <Button
                            key={index}
                            variant={action.variant}
                            size={action.size}
                            disabled={action.disabled}
                            onClick={action.onClick}
                        >
                            {action.label}
                        </Button>
                    ))}
                </div>
            </div>
        </div>
    );
}
