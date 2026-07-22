import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageHeader, LoadingState, ErrorState, EmptyState } from '@/components/layout';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Plus, BrainCircuit, Clock } from 'lucide-react';
import { useDeliverableCounts } from '@/features/dashboard/hooks/useDeliverableCounts';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';
import {
    DELIVERABLE_STATUS_COLORS,
    DELIVERABLE_STATUS_TEXT_COLORS,
    getStatusColor,
    getStatusTextColor,
} from '@/lib/constants';
import { StatCard } from '@/features/dashboard/components/StatCard';

export function DashboardPage() {
    const navigate = useNavigate();
    const {
        deliverablesDraft,
        deliverablesDesign,
        deliverablesPlan,
        deliverablesImplement,
        deliverablesMerge,
        deliverablesDeploy,
        deliverablesTest,
        deliverablesDone,
        deliverablesNeedsReview,
        deliverablesFailed,
        deliverablesRejected,
        loading,
        error,
        refetch,
    } = useDeliverableCounts();

    const [showCreateProject, setShowCreateProject] = useState(false);

    const hasData = deliverablesDraft > 0 || deliverablesDesign > 0 || deliverablesPlan > 0 || deliverablesImplement > 0 || deliverablesMerge > 0 || deliverablesDeploy > 0 || deliverablesTest > 0 || deliverablesDone > 0 || deliverablesNeedsReview > 0 || deliverablesFailed > 0 || deliverablesRejected > 0;

    const statusCounts = [
        { status: 'DRAFT', count: deliverablesDraft },
        { status: 'DESIGN', count: deliverablesDesign },
        { status: 'PLAN', count: deliverablesPlan },
        { status: 'IMPLEMENT', count: deliverablesImplement },
        { status: 'MERGE', count: deliverablesMerge },
        { status: 'DEPLOY', count: deliverablesDeploy },
        { status: 'TEST', count: deliverablesTest },
        { status: 'DONE', count: deliverablesDone },
        { status: 'NEEDS_REVIEW', count: deliverablesNeedsReview },
        { status: 'FAILED', count: deliverablesFailed },
        { status: 'REJECTED', count: deliverablesRejected },
    ];

    if (loading) {
        return (
            <div className="space-y-6">
                <PageHeader title="Dashboard" description="Welcome to your DevStack dashboard." />
                <LoadingState cards={2} rows={4} />
            </div>
        );
    }

    if (error) {
        return (
            <div className="space-y-6">
                <PageHeader title="Dashboard" description="Welcome to your DevStack dashboard." />
                <ErrorState
                    message={error.message}
                    onRetry={() => refetch()}
                />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <PageHeader
                title="Dashboard"
                description="Welcome to your DevStack dashboard."
                actionSlot={<Button onClick={() => setShowCreateProject(true)}><Plus className="h-4 w-4 mr-2" />New Project</Button>}
            />

            {!hasData && (
                <EmptyState
                    description="No data available yet. Create your first project to get started."
                    action={{ label: 'Create Project', onClick: () => setShowCreateProject(true) }}
                />
            )}

            {hasData && (
                <div className="flex gap-2">
                    {deliverablesNeedsReview > 0 && (
                        <Button variant="outline" onClick={() => navigate('/deliverables?status=NEEDS_REVIEW')}>
                            <BrainCircuit className="h-4 w-4 mr-2" />
                            Review Deliverables ({deliverablesNeedsReview})
                        </Button>
                    )}
                    {deliverablesImplement > 0 && (
                        <Button variant="outline" onClick={() => navigate('/deliverables?status=IMPLEMENT')}>
                            <Clock className="h-4 w-4 mr-2" />
                            View In Progress ({deliverablesImplement})
                        </Button>
                    )}
                </div>
            )}

            <Card>
                <CardHeader>
                    <CardTitle>Count of Deliverables per Status</CardTitle>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Count</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {statusCounts.map(({ status, count }) => (
                                <TableRow key={status}>
                                    <TableCell>
                                        <Badge className={`${getStatusColor(status, DELIVERABLE_STATUS_COLORS)} ${getStatusTextColor(status, DELIVERABLE_STATUS_TEXT_COLORS)}`}>
                                            {status.replace('_', ' ')}
                                        </Badge>
                                    </TableCell>
                                    <TableCell className="text-right font-medium">{count}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Design"
                    value={deliverablesDesign}
                    variant="default"
                    description="Deliverables in design"
                />
                <StatCard
                    title="Plan"
                    value={deliverablesPlan}
                    variant="default"
                    description="Deliverables in planning"
                />
                <StatCard
                    title="Implement"
                    value={deliverablesImplement}
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
