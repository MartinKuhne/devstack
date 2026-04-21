import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useDeliverable } from '../hooks/useDeliverable';
import { EditDeliverableDialog } from '../components/EditDeliverableDialog';
import { useState } from 'react';
import { useTransitionDeliverableStatusMutation } from '@/generated/graphql';
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

const STATUS_COLORS: Record<string, string> = {
    DRAFT: 'bg-gray-500',
    PLANNING: 'bg-blue-500',
    READY: 'bg-green-500',
    IN_PROGRESS: 'bg-yellow-500',
    NEEDS_REVIEW: 'bg-purple-500',
    DONE: 'bg-emerald-600',
    FAILED: 'bg-red-500',
    REJECTED: 'bg-gray-600',
};

export function DeliverableDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { deliverable, loading, error, refetch } = useDeliverable(id ?? '');
    const { agentTasks, loading: agentTasksLoading, error: agentTasksError, refetch: refetchAgentTasks } = useAgentTasksByDeliverable(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
    const [transitionDeliverableStatus, { loading: transitionLoading }] = useTransitionDeliverableStatusMutation();
    const [selectedStatus, setSelectedStatus] = useState('');
    const [createAgentTaskDialogOpen, setCreateAgentTaskDialogOpen] = useState(false);
    const [deleting, setDeleting] = useState(false);

    const handleDelete = async () => {
        if (!deliverable?.id) return;
        if (!confirm('Are you sure you want to delete this deliverable? This action cannot be undone.')) return;
        setDeleting(true);
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: 'mutation DeleteDeliverable($input: DeleteDeliverableInput!) { deleteDeliverable(input: $input) { deliverable { id } errors } }',
                    variables: { input: { id: deliverable.id } },
                }),
            });
            const result = await response.json();
            if (result?.data?.deleteDeliverable?.errors?.length) {
                toast.error(result.data.deleteDeliverable.errors.join(', '));
            } else {
                toast.success('Deliverable deleted successfully');
                navigate('/deliverables');
            }
        } catch {
            toast.error('Failed to delete deliverable');
        } finally {
            setDeleting(false);
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
                        <CardTitle className="text-destructive">Error loading deliverable</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error?.message ?? 'Deliverable not found'}</p>
                        <Button variant="outline" className="mt-4" onClick={() => navigate('/deliverables')}>
                            Back to Deliverables
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    const validTransitions = deliverable.validStatusTransitions?.filter(s => s !== null) || [];

    const handleStatusChange = async () => {
        if (!selectedStatus || !deliverable.id) return;

        try {
            const result = await transitionDeliverableStatus({
                variables: {
                    input: {
                        id: deliverable.id,
                        targetStatus: selectedStatus as never,
                        actor: 'admin-ui',
                    },
                },
            });

            if (result.data?.transitionDeliverableStatus?.errors?.length) {
                toast.error(result.data.transitionDeliverableStatus.errors.join(', '));
                return;
            }

            toast.success('Status updated successfully');
            refetch();
            setSelectedStatus('');
        } catch {
            toast.error('Failed to update status');
        }
    };

    return (
        <div className="space-y-6">
            <table className="w-full border-collapse" style={{ tableLayout: 'fixed' }}>
                <thead>
                    <tr>
                        <th className="text-left p-2 text-lg font-semibold" colSpan={3} style={{ width: '70%' }}>
                            {deliverable.title}
                        </th>
                        <th className="text-center p-2 border-l">
                            <span className="text-xs uppercase text-muted-foreground block mb-1">Type</span>
                            <span>{deliverable.subtype}</span>
                        </th>
                        <th className="text-center p-2 border-l">
                            <span className="text-xs uppercase text-muted-foreground block mb-1">Status</span>
                            <Badge className={STATUS_COLORS[deliverable.status ?? ''] || 'bg-gray-500'}>
                                {deliverable.status}
                            </Badge>
                        </th>
                    </tr>
                </thead>
            </table>

            <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => navigate('/deliverables')}>
                    Back to List
                </Button>
                <Button onClick={() => setUpdateDialogOpen(true)}>
                    Edit
                </Button>
                <Button variant="destructive" onClick={handleDelete} disabled={deleting}>
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
                                    {validTransitions.map((status) => (
                                        <SelectItem key={status ?? ''} value={status ?? ''}>
                                            {status}
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
                                <p className="text-sm whitespace-pre-wrap">{deliverable.description}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.acceptanceCriteria && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Acceptance Criteria</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.acceptanceCriteria}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.plan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Execution Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.plan}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.securityImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Security Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.securityImpact}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.performanceImpact && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Performance Impact</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.performanceImpact}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.testPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Test Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.testPlan}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.deploymentPlan && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Deployment Plan</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.deploymentPlan}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.result && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Agent Feedback</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.result}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.openQuestions && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Blocking</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.openQuestions}</p>
                            </CardContent>
                        </Card>
                    )}
                </div>

                <div className="lg:col-span-1">
                    <Card className="sticky top-4">
                        <CardHeader>
                            <div className="flex items-center justify-between">
                                <CardTitle>Agent Tasks</CardTitle>
                                <Button onClick={() => setCreateAgentTaskDialogOpen(true)} size="sm">
                                    New
                                </Button>
                            </div>
                        </CardHeader>
                        <CardContent>
                            {agentTasksError ? (
                                <p className="text-sm text-destructive">{agentTasksError.message}</p>
                            ) : agentTasksLoading ? (
                                <div className="space-y-2">
                                    {[1, 2, 3].map((item) => (
                                        <Skeleton key={item} className="h-10 w-full" />
                                    ))}
                                </div>
                            ) : agentTasks && agentTasks.length > 0 ? (
                                <div className="space-y-2">
                                    {agentTasks.map((task) => (
                                        <div
                                            key={task.id ?? ''}
                                            className="flex items-center justify-between p-2 rounded-md hover:bg-muted/50 cursor-pointer"
                                            onClick={() => task.id && navigate(`/agent-tasks/${task.id}`)}
                                        >
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium truncate">{task.title}</p>
                                                <p className="text-xs text-muted-foreground">
                                                    {task.model || '-'} · {(task.promptTokens ?? 0) + (task.completionTokens ?? 0)} tokens
                                                </p>
                                            </div>
                                            <Badge className={STATUS_COLORS[task.status ?? ''] || 'bg-gray-500'}>
                                                {task.status}
                                            </Badge>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <p className="text-muted-foreground text-sm">No agent tasks for this deliverable.</p>
                            )}
                        </CardContent>
                    </Card>
                </div>
            </div>

            <div className="text-sm text-muted-foreground">
                <p>Created: {deliverable.createdAt ? new Date(deliverable.createdAt).toLocaleString() : '-'}</p>
                <p>Updated: {deliverable.updatedAt ? new Date(deliverable.updatedAt).toLocaleString() : '-'}</p>
            </div>

            <EditDeliverableDialog
                open={updateDialogOpen}
                onOpenChange={setUpdateDialogOpen}
                deliverable={{
                    id: deliverable.id ?? '',
                    title: deliverable.title ?? '',
                    description: deliverable.description,
                    acceptanceCriteria: deliverable.acceptanceCriteria,
                    executionPlan: deliverable.plan,
                    securityImpact: deliverable.securityImpact,
                    performanceImpact: deliverable.performanceImpact,
                    testPlan: deliverable.testPlan,
                    deploymentPlan: deliverable.deploymentPlan,
                    blocking: deliverable.openQuestions,
                }}
                onSuccess={() => refetch()}
            />
            <CreateAgentTaskDialog
                open={createAgentTaskDialogOpen}
                onOpenChange={setCreateAgentTaskDialogOpen}
                projectId={deliverable.projectId ?? ''}
                itemId={id ?? ''}
                onSuccess={() => {
                    refetchAgentTasks();
                    refetch();
                }}
            />
        </div>
    );
}
