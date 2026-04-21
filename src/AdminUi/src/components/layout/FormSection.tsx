import { Label } from '@/components/ui/label';
import type { ReactNode } from 'react';

export type FormFieldProps = {
    label: string;
    description?: string;
    required?: boolean;
    error?: string;
    children: ReactNode;
    className?: string;
};

export function FormField({ label, description, required, error, children, className }: FormFieldProps) {
    return (
        <div className={className}>
            <Label className="block mb-1.5 text-sm font-medium">
                {label}
                {required && <span className="text-destructive ml-0.5">*</span>}
            </Label>
            {description && <p className="text-xs text-muted-foreground mb-2">{description}</p>}
            {children}
            {error && <p className="text-xs text-destructive mt-1">{error}</p>}
        </div>
    );
}

export type FormSectionProps = {
    title?: string;
    description?: string;
    children: ReactNode;
    className?: string;
    gap?: 'sm' | 'default' | 'lg';
};

const gapMap = {
    sm: 'gap-3',
    default: 'gap-4',
    lg: 'gap-6',
};

export function FormSection({ title, description, children, className, gap = 'default' }: FormSectionProps) {
    return (
        <div className={className}>
            {title && (
                <div className="mb-4 space-y-1">
                    <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                        {title}
                    </h3>
                    {description && <p className="text-sm text-muted-foreground">{description}</p>}
                </div>
            )}
            <div className={gapMap[gap]}>
                {children}
            </div>
        </div>
    );
}

export type FormGridProps = {
    cols?: 1 | 2 | 3;
    children: ReactNode;
    className?: string;
};

const colMap = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 sm:grid-cols-2',
    3: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
};

export function FormGrid({ cols = 2, children, className }: FormGridProps) {
    return (
        <div className={`${colMap[cols]} grid gap-4 ${className}`}>
            {children}
        </div>
    );
}
