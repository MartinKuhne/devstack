import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { RefreshCw, Plus, AlertTriangle, Clock } from 'lucide-react';
import { useDashboardSummary } from '@/features/dashboard/hooks/useDashboardSummary';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';
import type { GetDashboardSummaryQuery } from '@/generated/graphql';

interface StatCardProps {
    title: string;
    value: number;
    variant: 'default' | 'warning' | 'danger';
    description: string;
}

export function StatCard({ title, value, variant, description }: StatCardProps) {
    const badgeVariant = variant === 'danger' ? 'destructive' : variant === 'warning' ? 'secondary' : 'default';
    
    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{title}</CardTitle>
                <Badge variant={badgeVariant}>{value}</Badge>
            </CardHeader>
            <CardContent>
                <div className="text-2xl font-bold">{value}</div>
                <p className="text-xs text-muted-foreground">{description}</p>
            </CardContent>
        </Card>
    );
}

export function DashboardPage() {
    const navigate = useNavigate();
    const { dashboardSummary, loading, error, refetch, isBackgroundRefresh } = useDashboardSummary();
    const [showCreateProject, setShowCreateProject] = useState(false);

    const handleRefresh = async () => {
        await refetch();
    };

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                </div>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
                    {[1, 2, 3, 4, 5].map((item) => (
                        <Card key={item}>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Metric {item}</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    <Skeleton className="h-8 w-16" />
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
                <Card>
                    <CardHeader>
                        <Skeleton className="h-6 w-48" />
                    </CardHeader>
                    <CardContent>
                        <Skeleton className="h-32 w-full" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error) {
        return (
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                        <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                    </div>
                    <Button onClick={handleRefresh} disabled={loading}>
                        <RefreshCw className="h-4 w-4" />
                        Refresh
                    </Button>
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

    const summary = dashboardSummary as GetDashboardSummaryQuery['dashboardSummary'] | undefined;
    const hasData = summary && (summary.projectsInFlight > 0 || summary.featuresInReview > 0 || summary.featuresFailed > 0 || summary.tasksInProgress > 0 || summary.tasksFailed > 0);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                </div>
                <div className="flex items-center gap-2">
                    {isBackgroundRefresh && (
                        <div className="flex items-center text-xs text-muted-foreground">
                            <Clock className="h-3 w-3 mr-1 animate-pulse" />
                            Refreshing...
                        </div>
                    )}
                    <Button onClick={handleRefresh} disabled={loading}>
                        <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
                        Refresh
                    </Button>
                    <Button onClick={() => setShowCreateProject(true)}>
                        <Plus className="h-4 w-4 mr-2" />
                        New Project
                    </Button>
                </div>
            </div>
            
            {hasData === false && (
                <Card>
                    <CardContent className="pt-6">
                        <p className="text-center text-muted-foreground">No data available yet. Create your first project to get started.</p>
                    </CardContent>
                </Card>
            )}

            {hasData && (
                <div className="flex gap-2">
                    {(summary?.featuresFailed ?? 0) > 0 && (
                        <Button variant="outline" onClick={() => navigate('/features?status=Failed')}>
                            <AlertTriangle className="h-4 w-4 mr-2" />
                            View Failed Features ({summary.featuresFailed})
                        </Button>
                    )}
                    {(summary?.tasksFailed ?? 0) > 0 && (
                        <Button variant="outline" onClick={() => navigate('/tasks?status=Failed')}>
                            <AlertTriangle className="h-4 w-4 mr-2" />
                            View Failed Tasks ({summary.tasksFailed})
                        </Button>
                    )}
                </div>
            )}

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
                <StatCard
                    title="Projects In Flight"
                    value={summary?.projectsInFlight ?? 0}
                    variant="default"
                    description="Active projects"
                />
                <StatCard
                    title="Features In Review"
                    value={summary?.featuresInReview ?? 0}
                    variant="warning"
                    description="Features awaiting review"
                />
                <StatCard
                    title="Features Failed"
                    value={summary?.featuresFailed ?? 0}
                    variant="danger"
                    description="Features that failed"
                />
                <StatCard
                    title="Tasks In Progress"
                    value={summary?.tasksInProgress ?? 0}
                    variant="default"
                    description="Tasks in progress"
                />
                <StatCard
                    title="Tasks Failed"
                    value={summary?.tasksFailed ?? 0}
                    variant="danger"
                    description="Tasks that failed"
                />
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Recent Activity</CardTitle>
                </CardHeader>
                <CardContent>
                    {summary?.recentAuditEvents && summary.recentAuditEvents.length > 0 ? (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Entity</TableHead>
                                    <TableHead>Event</TableHead>
                                    <TableHead>Actor</TableHead>
                                    <TableHead>Time</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {summary.recentAuditEvents.map((event) => (
                                    <TableRow key={event.id}>
                                        <TableCell>{event.entityType}</TableCell>
                                        <TableCell>{event.eventType}</TableCell>
                                        <TableCell>{event.actor ?? '-'}</TableCell>
                                        <TableCell>{new Date(event.occurredAt).toLocaleString()}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    ) : (
                        <p className="text-muted-foreground text-sm">No recent activity</p>
                    )}
                </CardContent>
            </Card>

            <CreateProjectDialog
                open={showCreateProject}
                onOpenChange={setShowCreateProject}
                onSuccess={() => refetch()}
            />
        </div>
    );
}
