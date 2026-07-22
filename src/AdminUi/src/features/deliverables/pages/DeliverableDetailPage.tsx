import { useParams, useNavigate, Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useDeliverable } from '../hooks/useDeliverable';
import { useDeleteDeliverable } from '../hooks/useDeleteDeliverable';
import { EditDeliverableDialog } from '../components/EditDeliverableDialog';
import { useState } from 'react';
import { useUpdateDeliverableStatusMutation, DeliverableStatus } from '@/generated/graphql';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { toast } from 'react-toastify';
import { CreateAgentTaskDialog } from '@/features/agentTasks/components/CreateAgentTaskDialog';
import {
    DELIVERABLE_STATUS_COLORS,
    DELIVERABLE_STATUS_TEXT_COLORS,
    AGENT_TASK_STATUS_COLORS,
    AGENT_TASK_STATUS_TEXT_COLORS,
    getStatusColor,
    getStatusTextColor,
    getStatusIcon,
} from '@/lib/constants';
import { MarkdownViewer } from '@/components/MarkdownViewer';
import { LoadingState, ErrorState, EmptyState, DetailLayout, ActivityTimeline } from '@/components/layout';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { Skeleton } from '@/components/ui/skeleton';

export function DeliverableDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { deliverable, loading, error, refetch } = useDeliverable(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
    const [updateDeliverableStatus, { loading: transitionLoading }] =
        useUpdateDeliverableStatusMutation();
    const { deleteDeliverable, loading: deleteLoading } = useDeleteDeliverable();
    const [selectedStatus, setSelectedStatus] = useState('');
    const [createAgentTaskDialogOpen, setCreateAgentTaskDialogOpen] = useState(false);
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

    const handleDelete = async () => {
        if (!deliverable?.id) return;

        const result = await deleteDeliverable(deliverable.id);

        if (result.success) {
            toast.success('Deliverable deleted successfully');
            navigate('/deliverables');
        } else {
            toast.error(result.errors?.join(', ') ?? 'Failed to delete deliverable');
        }
        setConfirmDeleteOpen(false);
    };

    const renderStatusIcon = (status: string | undefined, entity: 'deliverable' | 'project' | 'agentTask') => {
        const Icon = getStatusIcon(status, entity);
        return Icon ? <Icon className="mr-1 h-3 w-3" /> : null;
    };

    const handleRowKeyDown = (e: React.KeyboardEvent, path: string) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            navigate(path);
        }
    };

    if (loading) {
        return (
            <div className="space-y-6">
                <DetailLayout
                    breadcrumbs={[{ label: 'Deliverables', to: '/deliverables' }, { label: 'Loading...' }]}
                    title="Loading..."
                />
                <LoadingState cards={1} rows={3} />
            </div>
        );
    }

    if (error || !deliverable) {
        return (
            <DetailLayout
                breadcrumbs={[{ label: 'Deliverables', to: '/deliverables' }, { label: 'Error' }]}
                title="Deliverable"
            >
                <ErrorState
                    message={error?.message ?? 'Deliverable not found'}
                    onRetry={() => refetch()}
                />
            </DetailLayout>
        );
    }

    const allStatuses = Object.values(DeliverableStatus);
    const filteredStatuses = allStatuses.filter((s) => s !== deliverable.status);

    const handleStatusChange = async () => {
        if (!selectedStatus || !deliverable.id) return;

        try {
            await updateDeliverableStatus({
                variables: {
                    id: deliverable.id,
                    targetStatus: selectedStatus as DeliverableStatus,
                },
            });

            toast.success('Status updated successfully');
            refetch();
            setSelectedStatus('');
        } catch {
            toast.error('Failed to update status');
        }
    };

    return (
        <DetailLayout
            breadcrumbs={[
                deliverable.projectId ? { label: 'Projects', to: '/projects' } : { label: 'Deliverables', to: '/deliverables' },
                deliverable.projectId ? { label: 'Deliverables', to: `/deliverables?project=${deliverable.projectId}` } : { label: deliverable.title ?? 'Deliverable' },
                { label: deliverable.title ?? 'Deliverable' },
            ].slice(0, deliverable.projectId ? 3 : 2)}
            title={deliverable.title ?? 'Deliverable'}
            typeLabel={deliverable.type}
            statusNode={
                <Badge className={`${getStatusColor(deliverable.status ?? undefined, DELIVERABLE_STATUS_COLORS)} ${getStatusTextColor(deliverable.status ?? undefined, DELIVERABLE_STATUS_TEXT_COLORS)}`}>
                    {renderStatusIcon(deliverable.status ?? undefined, 'deliverable')}
                    {deliverable.status}
                </Badge>
            }
            actions={
                <>
                    <Button variant="outline" onClick={() => navigate('/deliverables')}>
                        Back to List
                    </Button>
                    <Button onClick={() => setUpdateDialogOpen(true)}>Edit</Button>
                    <Button variant="destructive" onClick={() => setConfirmDeleteOpen(true)} disabled={deleteLoading}>
                        Delete
                    </Button>
                </>
            }
        >
            <Card>
                <CardHeader>
                    <CardTitle>Change Status</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex gap-2 items-end">
                        <div className="flex-1">
                            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
                                <SelectTrigger>
                                    <SelectValue placeholder="Select new status" />
                                </SelectTrigger>
                                <SelectContent>
                                    {filteredStatuses.map((status) => (
                                        <SelectItem key={status} value={status}>
                                            {status.replace(/_/g, ' ')}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>
                        <Button
                            onClick={handleStatusChange}
                            disabled={!selectedStatus || transitionLoading}
                        >
                            {transitionLoading ? 'Updating...' : 'Update Status'}
                        </Button>
                    </div>
                </CardContent>
            </Card>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 space-y-4">
                    {deliverable.description && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Description</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.description} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.acceptanceCriteria && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Acceptance Criteria</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.acceptanceCriteria} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.executionPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Execution Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.executionPlan} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.securityImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Security Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.securityImpact} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.performanceImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Performance Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.performanceImpact} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.testPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Test Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.testPlan} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.deploymentPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Deployment Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.deploymentPlan} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.agentFeedback && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Agent Feedback</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.agentFeedback} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.blocking && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Blocking</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.blocking} />
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.design && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Design</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer className="text-sm" content={deliverable.design} />
                            </CardContent>
                        </Card>
                    )}
                </div>

                <div className="lg:col-span-1">
                    <Card className="sticky top-4">
                        <CardHeader>
                            <div className="flex items-center justify-between">
                                <CardTitle>Agent Tasks</CardTitle>
                                <Button
                                    onClick={() => setCreateAgentTaskDialogOpen(true)}
                                    size="sm"
                                >
                                    New
                                </Button>
                            </div>
                        </CardHeader>
                        <CardContent>
                            {loading ? (
                                <div className="space-y-2">
                                    {[1, 2, 3].map((item) => (
                                        <Skeleton key={item} className="h-10 w-full animate-pulse" />
                                    ))}
                                </div>
                            ) : deliverable.agentTasks && deliverable.agentTasks.length > 0 ? (
                                <div className="space-y-2">
                                    {deliverable.agentTasks.map((task) =>
                                        task ? (
                                            <div
                                                key={task.id ?? ''}
                                                className="flex items-center justify-between p-2 rounded-md hover:bg-muted/50 cursor-pointer"
                                                tabIndex={0}
                                                role="button"
                                                onClick={() =>
                                                    task.id && navigate(`/agent-tasks/${task.id}`)
                                                }
                                                onKeyDown={(e) =>
                                                    task.id && handleRowKeyDown(e, `/agent-tasks/${task.id}`)
                                                }
                                            >
                                                <div className="flex-1 min-w-0">
                                                    <p className="text-sm font-medium truncate">
                                                        {task.title}
                                                    </p>
                                                    <p className="text-xs text-muted-foreground">
                                                        {(task.promptTokens ?? 0) +
                                                            (task.completionTokens ?? 0)}{' '}
                                                        tokens
                                                    </p>
                                                </div>
                                                <Badge
                                                    className={`${getStatusColor(
                                                        task.status ?? undefined,
                                                        AGENT_TASK_STATUS_COLORS
                                                    )} ${getStatusTextColor(
                                                        task.status ?? undefined,
                                                        AGENT_TASK_STATUS_TEXT_COLORS
                                                    )}`}
                                                >
                                                    {renderStatusIcon(task.status ?? undefined, 'agentTask')}
                                                    {task.status}
                                                </Badge>
                                            </div>
                                        ) : null
                                    )}
                                </div>
                            ) : (
                                <EmptyState
                                    description="No agent tasks for this deliverable."
                                    action={{ label: 'New', onClick: () => setCreateAgentTaskDialogOpen(true) }}
                                />
                            )}
                        </CardContent>
                    </Card>
                </div>
            </div>

            {deliverable.projectId && (
                <div className="text-sm text-muted-foreground">
                    <p>
                        Project:{' '}
                        <Link to={`/projects/${deliverable.projectId}`} className="text-blue-600 hover:underline">
                            View project
                        </Link>
                    </p>
                </div>
            )}

            <ActivityTimeline events={[]} />

            <EditDeliverableDialog
                open={updateDialogOpen}
                onOpenChange={setUpdateDialogOpen}
                deliverable={{
                    id: deliverable.id ?? '',
                    title: deliverable.title ?? '',
                    description: deliverable.description,
                    acceptanceCriteria: deliverable.acceptanceCriteria,
                    executionPlan: deliverable.executionPlan,
                    securityImpact: deliverable.securityImpact,
                    performanceImpact: deliverable.performanceImpact,
                    testPlan: deliverable.testPlan,
                    deploymentPlan: deliverable.deploymentPlan,
                    blocking: deliverable.blocking,
                    design: deliverable.design,
                }}
                onSuccess={() => {
                    toast.success('Deliverable updated successfully');
                    refetch();
                }}
            />
            <CreateAgentTaskDialog
                open={createAgentTaskDialogOpen}
                onOpenChange={setCreateAgentTaskDialogOpen}
                deliverableId={id ?? ''}
                projectId={deliverable?.projectId ?? ''}
                onSuccess={() => {
                    toast.success('Agent task created successfully');
                    refetch();
                }}
            />
            <ConfirmDialog
                open={confirmDeleteOpen}
                onOpenChange={setConfirmDeleteOpen}
                title="Delete Deliverable"
                description="Are you sure you want to delete this deliverable? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />
        </DetailLayout>
    );
}
