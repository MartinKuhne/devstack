import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { ReactNode } from 'react';

export type DataPanelProps = {
    title?: string;
    children: ReactNode;
    className?: string;
    headerClassName?: string;
};

export function DataPanel({ title, children, className, headerClassName }: DataPanelProps) {
    return (
        <Card className={className}>
            {title && (
                <CardHeader className={headerClassName}>
                    <CardTitle>{title}</CardTitle>
                </CardHeader>
            )}
            <CardContent className={title ? undefined : ''}>{children}</CardContent>
        </Card>
    );
}
