import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
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
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">{deliverable.title}</h2>
                    <div className="flex items-center gap-2 mt-2">
                        <Badge className={STATUS_COLORS[deliverable.status ?? ''] || 'bg-gray-500'}>
                            {deliverable.status}
                        </Badge>
                        <span className="text-sm text-muted-foreground">
                            {deliverable.subtype}
                        </span>
                    </div>
                </div>
                <div className="flex gap-2">
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

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="details">Details</TabsTrigger>
                    <TabsTrigger value="agent-tasks">Agent Tasks</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
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

                    {deliverable.result && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Result</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.result}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.errors && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-destructive">Errors</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm text-destructive whitespace-pre-wrap">{deliverable.errors}</p>
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Created: {deliverable.createdAt ? new Date(deliverable.createdAt).toLocaleString() : '-'}</p>
                        <p>Updated: {deliverable.updatedAt ? new Date(deliverable.updatedAt).toLocaleString() : '-'}</p>
                    </div>
                </TabsContent>

                <TabsContent value="details" className="space-y-4">
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

                    {deliverable.openQuestions && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Open Questions</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.openQuestions}</p>
                            </CardContent>
                        </Card>
                    )}

                    {deliverable.rootCause && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Root Cause</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{deliverable.rootCause}</p>
                            </CardContent>
                        </Card>
                    )}
                </TabsContent>

                <TabsContent value="agent-tasks">
                    <Card>
                        <CardHeader>
                            <div className="flex items-center justify-between">
                                <CardTitle>Agent Tasks</CardTitle>
                                <Button onClick={() => setCreateAgentTaskDialogOpen(true)}>
                                    New Agent Task
                                </Button>
                            </div>
                        </CardHeader>
                        <CardContent>
                            {agentTasksError ? (
                                <p className="text-sm text-destructive">{agentTasksError.message}</p>
                            ) : agentTasksLoading ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Model</TableHead>
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[1, 2, 3].map((item) => (
                                            <TableRow key={item}>
                                                <TableCell><Skeleton className="h-4 w-64" /></TableCell>
                                                <TableCell><Skeleton className="h-6 w-20" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : agentTasks && agentTasks.length > 0 ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Model</TableHead>
                                            <TableHead>Tokens</TableHead>
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {agentTasks.map((task) => (
                                            <TableRow
                                                key={task.id ?? ''}
                                                className="cursor-pointer hover:bg-muted/50"
                                                onClick={() => task.id && navigate(`/agent-tasks/${task.id}`)}
                                            >
                                                <TableCell className="font-medium">{task.title}</TableCell>
                                                <TableCell>
                                                    <Badge className={STATUS_COLORS[task.status ?? ''] || 'bg-gray-500'}>
                                                        {task.status}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell>{task.model || '-'}</TableCell>
                                                <TableCell>
                                                    {(task.promptTokens ?? 0) + (task.completionTokens ?? 0)}
                                                </TableCell>
                                                <TableCell>
                                                    {task.updatedAt ? new Date(task.updatedAt).toLocaleDateString() : '-'}
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : (
                                <p className="text-muted-foreground text-sm">No agent tasks for this deliverable.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>

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
