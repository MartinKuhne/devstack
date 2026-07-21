import { Button } from '@/components/ui/button';
import { AlertCircle } from 'lucide-react';

export type ErrorStateProps = {
    title?: string;
    message: string;
    detail?: string;
    onRetry?: () => void;
    retryLabel?: string;
    className?: string;
};

export function ErrorState({
    title = 'Error',
    message,
    detail,
    onRetry,
    retryLabel = 'Retry',
    className,
}: ErrorStateProps) {
    return (
        <div className={className} role="alert">
            <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-6">
                <div className="flex items-start gap-3">
                    <AlertCircle className="h-5 w-5 text-destructive mt-0.5 shrink-0" />
                    <div className="space-y-1 flex-1">
                        <h3 className="font-semibold text-destructive">{title}</h3>
                        <p className="text-sm text-destructive">{message}</p>
                        {detail && <p className="text-xs text-muted-foreground font-mono">{detail}</p>}
                        {onRetry && (
                            <div className="pt-2">
                                <Button variant="outline" size="sm" onClick={onRetry}>
                                    {retryLabel}
                                </Button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
