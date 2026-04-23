import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
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
import { useAgentTasksByDeliverable } from '@/features/agentTasks/hooks/useAgentTasksByDeliverable';
import { CreateAgentTaskDialog } from '@/features/agentTasks/components/CreateAgentTaskDialog';
import {
    DELIVERABLE_STATUS_COLORS,
    AGENT_TASK_STATUS_COLORS,
    getStatusColor,
} from '@/lib/constants';
export function DeliverableDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { deliverable, loading, error, refetch } = useDeliverable(id ?? '');
    const {
        agentTasks,
        loading: agentTasksLoading,
        error: agentTasksError,
        refetch: refetchAgentTasks,
    } = useAgentTasksByDeliverable(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
    const [updateDeliverableStatus, { loading: transitionLoading }] =
        useUpdateDeliverableStatusMutation();
    const { deleteDeliverable, loading: deleteLoading } = useDeleteDeliverable();
    const [selectedStatus, setSelectedStatus] = useState('');
    const [createAgentTaskDialogOpen, setCreateAgentTaskDialogOpen] = useState(false);

    const handleDelete = async () => {
        if (!deliverable?.id) return;
        if (
            !confirm(
                'Are you sure you want to delete this deliverable? This action cannot be undone.'
            )
        )
            return;

        const result = await deleteDeliverable(deliverable.id);

        if (result.success) {
            toast.success('Deliverable deleted successfully');
            navigate('/deliverables');
        } else {
            toast.error(result.errors?.join(', ') ?? 'Failed to delete deliverable');
        }
    };

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <div className="h-8 w-64 bg-muted rounded" />
                    <div className="h-4 w-32 mt-2 bg-muted rounded" />
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Deliverable Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="h-20 bg-muted rounded" />
                        <div className="h-20 bg-muted rounded" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !deliverable) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Deliverable</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">
                            Error loading deliverable
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">
                            {error?.message ?? 'Deliverable not found'}
                        </p>
                        <Button
                            variant="outline"
                            className="mt-4"
                            onClick={() => navigate('/deliverables')}
                        >
                            Back to Deliverables
                        </Button>
                    </CardContent>
                </Card>
            </div>
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
        <div className="space-y-6">
            <div className="grid grid-cols-[1fr_auto_auto] gap-4 items-center border-b pb-4">
                <h1 className="text-2xl font-bold">{deliverable.title}</h1>
                <div className="text-center">
                    <span className="text-xs uppercase text-muted-foreground block mb-1">Type</span>
                    <span>{deliverable.type}</span>
                </div>
                <div className="text-center">
                    <span className="text-xs uppercase text-muted-foreground block mb-1">
                        Status
                    </span>
                    <Badge
                        className={getStatusColor(
                            deliverable.status ?? undefined,
                            DELIVERABLE_STATUS_COLORS
                        )}
                    >
                        {deliverable.status}
                    </Badge>
                </div>
            </div>

            <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => navigate('/deliverables')}>
                    Back to List
                </Button>
                <Button onClick={() => setUpdateDialogOpen(true)}>Edit</Button>
                <Button variant="destructive" onClick={handleDelete} disabled={deleteLoading}>
                    Delete
                </Button>
            </div>

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
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.description}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.acceptanceCriteria && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Acceptance Criteria</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.acceptanceCriteria}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.executionPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Execution Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.executionPlan}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.securityImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Security Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.securityImpact}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.performanceImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Performance Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.performanceImpact}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.testPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Test Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.testPlan}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.deploymentPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Deployment Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.deploymentPlan}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.agentFeedback && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Agent Feedback</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.agentFeedback}
                                </p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.blocking && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Blocking</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">
                                    {deliverable.blocking}
                                </p>
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
                            {agentTasksError ? (
                                <p className="text-sm text-destructive">
                                    {agentTasksError.message}
                                </p>
                            ) : agentTasksLoading ? (
                                <div className="space-y-2">
                                    {[1, 2, 3].map((item) => (
                                        <Skeleton key={item} className="h-10 w-full" />
                                    ))}
                                </div>
                            ) : agentTasks && agentTasks.length > 0 ? (
                                <div className="space-y-2">
                                    {agentTasks.map((task) =>
                                        task ? (
                                            <div
                                                key={task.id ?? ''}
                                                className="flex items-center justify-between p-2 rounded-md hover:bg-muted/50 cursor-pointer"
                                                onClick={() =>
                                                    task.id && navigate(`/agent-tasks/${task.id}`)
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
                                                    className={getStatusColor(
                                                        task.status ?? undefined,
                                                        AGENT_TASK_STATUS_COLORS
                                                    )}
                                                >
                                                    {task.status}
                                                </Badge>
                                            </div>
                                        ) : null
                                    )}
                                </div>
                            ) : (
                                <p className="text-muted-foreground text-sm">
                                    No agent tasks for this deliverable.
                                </p>
                            )}
                        </CardContent>
                    </Card>
                </div>
            </div>

            <div className="text-sm text-muted-foreground">
                <p>Deliverable ID: {deliverable.id ?? '-'}</p>
            </div>

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
                }}
                onSuccess={() => refetch()}
            />
            <CreateAgentTaskDialog
                open={createAgentTaskDialogOpen}
                onOpenChange={setCreateAgentTaskDialogOpen}
                deliverableId={id ?? ''}
                projectId={deliverable?.projectId ?? ''}
                onSuccess={() => {
                    refetchAgentTasks();
                    refetch();
                }}
            />
        </div>
    );
}
