import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useDashboardSummary } from '@/features/dashboard/hooks/useDashboardSummary';

export function DashboardPage() {
    const { dashboardSummary, loading, error } = useDashboardSummary();

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                </div>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                    {[1, 2, 3, 4].map((item) => (
                        <Card key={item}>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Total {item}</CardTitle>
                                <div className="h-4 w-4 rounded-full bg-muted" />
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    <Skeleton className="h-8 w-16" />
                                </div>
                                <p className="text-xs text-muted-foreground">
                                    <Skeleton className="h-3 w-24" />
                                </p>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading dashboard</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error.message}</p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
            </div>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Projects</CardTitle>
                        <div className="h-4 w-4 rounded-full bg-muted" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">
                            {dashboardSummary?.projectCount ?? 0}
                        </div>
                        <p className="text-xs text-muted-foreground">Total projects</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Features</CardTitle>
                        <div className="h-4 w-4 rounded-full bg-muted" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">
                            {dashboardSummary?.featureCount ?? 0}
                        </div>
                        <p className="text-xs text-muted-foreground">Total features</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Defects</CardTitle>
                        <div className="h-4 w-4 rounded-full bg-muted" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">
                            {dashboardSummary?.defectCount ?? 0}
                        </div>
                        <p className="text-xs text-muted-foreground">Total defects</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Tasks</CardTitle>
                        <div className="h-4 w-4 rounded-full bg-muted" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{dashboardSummary?.taskCount ?? 0}</div>
                        <p className="text-xs text-muted-foreground">Total tasks</p>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
}
