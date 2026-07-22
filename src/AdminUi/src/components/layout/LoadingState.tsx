import { Skeleton } from '@/components/ui/skeleton';

export type LoadingStateProps = {
    cards?: number;
    rows?: number;
    className?: string;
};

export function LoadingState({ cards = 1, rows = 3, className }: LoadingStateProps) {
    return (
        <div className={className} role="status" aria-busy="true">
            <span className="sr-only">Loading...</span>
            {Array.from({ length: cards }).map((_, cardIndex) => (
                <div key={cardIndex} className="space-y-4 mb-6 last:mb-0">
                    <div className="flex items-center gap-4">
                        <Skeleton className="h-6 w-32" />
                        <Skeleton className="h-4 w-48" />
                    </div>
                    <div className="rounded-lg border space-y-2">
                        {Array.from({ length: rows }).map((_, rowIndex) => (
                            <div key={rowIndex} className="flex items-center gap-4 p-4">
                                <Skeleton className="h-4 w-24" />
                                <Skeleton className="h-4 w-40" />
                                <Skeleton className="h-4 w-32" />
                                <Skeleton className="h-4 w-20 ml-auto" />
                            </div>
                        ))}
                    </div>
                </div>
            ))}
        </div>
    );
}
