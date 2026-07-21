import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Separator } from '@/components/ui/separator';
import { useAgentTask } from '../hooks/useAgentTask';
import { UpdateAgentTaskDialog } from '../components/UpdateAgentTaskDialog';
import { useState } from 'react';
import {
    useUpdateAgentTaskStatusMutation,
    useDeleteAgentTaskMutation,
    AgentTaskStatus,
} from '@/generated/graphql';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { toast } from 'react-toastify';
import { createModuleLogger } from '@/lib/logging';
import { AGENT_TASK_STATUS_COLORS, getStatusColor } from '@/lib/constants';

const logger = createModuleLogger('AgentTaskDetailPage');

export function AgentTaskDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { agentTask, loading, error, refetch } = useAgentTask(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
    const [updateAgentTaskStatus, { loading: transitionLoading }] =
        useUpdateAgentTaskStatusMutation();
    const [selectedStatus, setSelectedStatus] = useState('');
    const [deleteAgentTask, { loading: deleting }] = useDeleteAgentTaskMutation();

    const handleDelete = async () => {
        if (!agentTask?.id) return;
        if (
            !confirm(
                'Are you sure you want to delete this agent task? This action cannot be undone.'
            )
        )
            return;
        logger.info('Deleting agent task', { id: agentTask.id, title: agentTask.title });
        try {
            const result = await deleteAgentTask({
                variables: {
                    id: agentTask.id,
                },
            });
            const deleted = result.data?.deleteAgentTask;
            if (!deleted) {
                logger.warn('Failed to delete agent task', {
                    id: agentTask.id,
                });
                toast.error('Failed to delete agent task');
            } else {
                logger.info('Agent task deleted successfully', { id: agentTask.id });
                toast.success('Agent task deleted successfully');
                navigate('/agent-tasks');
            }
        } catch {
            logger.error('Failed to delete agent task', { id: agentTask.id });
            toast.error('Failed to delete agent task');
        }
    };

    const handleStatusChange = async () => {
        if (!selectedStatus || !agentTask?.id) return;

        try {
            await updateAgentTaskStatus({
                variables: {
                    id: agentTask.id,
                    targetStatus: selectedStatus as AgentTaskStatus,
                },
            });

            toast.success('Status updated successfully');
            refetch();
            setSelectedStatus('');
        } catch {
            toast.error('Failed to update status');
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
                        <CardTitle>Agent Task Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="h-20 bg-muted rounded" />
                        <div className="h-20 bg-muted rounded" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !agentTask) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Agent Task</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading agent task</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">
                            {error?.message ?? 'Agent task not found'}
                        </p>
                        <Button
                            variant="outline"
                            className="mt-4"
                            onClick={() => navigate('/agent-tasks')}
                        >
                            Back to Agent Tasks
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">{agentTask.title}</h2>
                    <div className="flex items-center gap-2 mt-2">
                        <Badge className={getStatusColor(agentTask.status ?? undefined, AGENT_TASK_STATUS_COLORS)}>
                            {agentTask.status}
                        </Badge>
                        <span className="text-sm text-muted-foreground">
                            Deliverable: {agentTask.deliverableId ?? '-'}
                        </span>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate('/agent-tasks')}>
                        Back to List
                    </Button>
                    <Button onClick={() => setUpdateDialogOpen(true)}>Edit</Button>
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
                                    {Object.values(AgentTaskStatus).map((status) => (
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

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="telemetry">Telemetry</TabsTrigger>
                    <TabsTrigger value="dependencies">Dependencies</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Description</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-sm whitespace-pre-wrap">
                                {agentTask.description ?? 'No description provided.'}
                            </p>
                        </CardContent>
                    </Card>

                    {agentTask.result && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Result</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{agentTask.result}</p>
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Complexity Rating: {agentTask.complexityRating ?? '-'}</p>
                        <p>Deliverable ID: {agentTask.deliverableId ?? '-'}</p>
                    </div>
                </TabsContent>

                <TabsContent value="telemetry" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Execution Telemetry</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <p className="text-sm text-muted-foreground">Agent</p>
                                    <p className="font-medium">{agentTask.agent || '-'}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Commit Hash</p>
                                    <p className="font-medium font-mono text-xs">
                                        {agentTask.commitHash || '-'}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Prompt Tokens</p>
                                    <p className="font-medium">{agentTask.promptTokens ?? 0}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">
                                        Completion Tokens
                                    </p>
                                    <p className="font-medium">{agentTask.completionTokens ?? 0}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">
                                        Execution Duration
                                    </p>
                                    <p className="font-medium">
                                        {agentTask.executionDurationInSeconds ?? 0} seconds
                                    </p>
                                </div>
                            </div>
                            {agentTask.errors && (
                                <>
                                    <Separator />
                                    <div>
                                        <p className="text-sm text-muted-foreground mb-2">Errors</p>
                                        <p className="text-sm text-destructive whitespace-pre-wrap">
                                            {agentTask.errors}
                                        </p>
                                    </div>
                                </>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="dependencies" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Dependencies</CardTitle>
                        </CardHeader>
                        <CardContent>
                            {agentTask.dependsOnAgentTaskId ? (
                                <div className="flex items-center gap-2 p-2 bg-muted rounded">
                                    <Badge variant="outline">
                                        {agentTask.dependsOnAgentTaskId}
                                    </Badge>
                                    <span className="text-sm">Depends on this task</span>
                                </div>
                            ) : (
                                <p className="text-sm text-muted-foreground">No dependencies.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>

            <UpdateAgentTaskDialog
                open={updateDialogOpen}
                onOpenChange={setUpdateDialogOpen}
                agentTask={{
                    id: agentTask.id ?? '',
                    title: agentTask.title ?? '',
                    description: agentTask.description ?? null,
                    complexityRating: agentTask.complexityRating ?? 0,
                    result: agentTask.result ?? null,
                    status: agentTask.status ?? null,
                    deliverableId: agentTask.deliverableId ?? null,
                }}
                onSuccess={() => refetch()}
            />
        </div>
    );
}
