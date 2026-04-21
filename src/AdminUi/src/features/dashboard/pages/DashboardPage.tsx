import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Plus, BrainCircuit, Clock } from 'lucide-react';
import { useDeliverableCounts } from '@/features/dashboard/hooks/useDeliverableCounts';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';

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
    const { deliverablesPlanning, deliverablesReady, deliverablesInProgress, deliverablesNeedsReview } = useDeliverableCounts();
    const [showCreateProject, setShowCreateProject] = useState(false);

    const hasData = deliverablesPlanning > 0 || deliverablesReady > 0 || deliverablesInProgress > 0 || deliverablesNeedsReview > 0;

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">Welcome to your DevStack dashboard.</p>
                </div>
                <div className="flex items-center gap-2">
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
                    {deliverablesNeedsReview > 0 && (
                        <Button variant="outline" onClick={() => navigate('/deliverables?status=NEEDS_REVIEW')}>
                            <BrainCircuit className="h-4 w-4 mr-2" />
                            Review Deliverables ({deliverablesNeedsReview})
                        </Button>
                    )}
                    {deliverablesInProgress > 0 && (
                        <Button variant="outline" onClick={() => navigate('/deliverables?status=IN_PROGRESS')}>
                            <Clock className="h-4 w-4 mr-2" />
                            View In Progress ({deliverablesInProgress})
                        </Button>
                    )}
                </div>
            )}

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Planning"
                    value={deliverablesPlanning}
                    variant="default"
                    description="Deliverables in planning"
                />
                <StatCard
                    title="Ready"
                    value={deliverablesReady}
                    variant="default"
                    description="Deliverables ready for execution"
                />
                <StatCard
                    title="In Progress"
                    value={deliverablesInProgress}
                    variant="warning"
                    description="Deliverables currently in progress"
                />
                <StatCard
                    title="Needs Review"
                    value={deliverablesNeedsReview}
                    variant="danger"
                    description="Deliverables awaiting review"
                />
            </div>

            <CreateProjectDialog
                open={showCreateProject}
                onOpenChange={setShowCreateProject}
            />
        </div>
    );
}
