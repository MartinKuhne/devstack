import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';

export function DefectListPage() {
    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-2xl font-bold tracking-tight">Defects</h2>
                <p className="text-muted-foreground">Track and manage defects.</p>
            </div>
            <Card>
                <CardHeader>
                    <CardTitle>Defect List</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="space-y-4">
                        {[1, 2, 3].map((item) => (
                            <div
                                key={item}
                                className="flex items-center justify-between py-2 border-b last:border-0"
                            >
                                <div className="space-y-1">
                                    <Skeleton className="h-4 w-64" />
                                    <Skeleton className="h-3 w-48" />
                                </div>
                                <div className="flex items-center gap-2">
                                    <Skeleton className="h-6 w-24" />
                                    <Skeleton className="h-6 w-20" />
                                </div>
                            </div>
                        ))}
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
