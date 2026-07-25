import { useParams, useNavigate, Link } from 'react-router';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Separator } from '@/components/ui/separator';
import { useAgentTask } from '../hooks/useAgentTask';
import { UpdateAgentTaskDialog } from '../components/UpdateAgentTaskDialog';
import { MarkdownViewer } from '@/components/MarkdownViewer';
import { useState, useCallback } from 'react';
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
import { AGENT_TASK_STATUS_COLORS, AGENT_TASK_STATUS_TEXT_COLORS, getStatusColor, getStatusTextColor, getStatusIcon } from '@/lib/constants';
import { Copy, Check, CheckCircle, XCircle, RotateCcw } from 'lucide-react';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { LoadingState, ErrorState, DetailLayout, ActivityTimeline } from '@/components/layout';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { Progress } from '@/components/ui/progress';

const logger = createModuleLogger('AgentTaskDetailPage');

const VALID_TRANSITIONS: Record<string, AgentTaskStatus[]> = {
    [AgentTaskStatus.READY]: [AgentTaskStatus.IN_PROGRESS, AgentTaskStatus.NEEDS_REVIEW],
    [AgentTaskStatus.IN_PROGRESS]: [
        AgentTaskStatus.NEEDS_REVIEW,
        AgentTaskStatus.DONE,
        AgentTaskStatus.FAILED,
    ],
    [AgentTaskStatus.NEEDS_REVIEW]: [AgentTaskStatus.DONE, AgentTaskStatus.REJECTED],
    [AgentTaskStatus.FAILED]: [AgentTaskStatus.READY],
    [AgentTaskStatus.DONE]: [AgentTaskStatus.READY],
    [AgentTaskStatus.REJECTED]: [AgentTaskStatus.READY],
};

export function AgentTaskDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { agentTask, loading, error, refetch } = useAgentTask(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
    const [updateAgentTaskStatus, { loading: transitionLoading }] =
        useUpdateAgentTaskStatusMutation();
    const [selectedStatus, setSelectedStatus] = useState('');
    const [deleteAgentTask, { loading: deleting }] = useDeleteAgentTaskMutation();
    const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
    const [rejectFeedback, setRejectFeedback] = useState('');
    const [errorsCopied, setErrorsCopied] = useState(false);
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

    const handleDelete = async () => {
        if (!agentTask?.id) return;
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
        setConfirmDeleteOpen(false);
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

    const transitionTo = useCallback(
        async (targetStatus: AgentTaskStatus) => {
            if (!agentTask?.id) return;
            try {
                await updateAgentTaskStatus({
                    variables: {
                        id: agentTask.id,
                        targetStatus,
                    },
                });
                toast.success('Status updated successfully');
                refetch();
            } catch {
                toast.error('Failed to update status');
            }
        },
        [agentTask, updateAgentTaskStatus, refetch]
    );

    const handleApprove = () => transitionTo(AgentTaskStatus.DONE);

    const handleReject = async () => {
        if (!agentTask?.id) return;
        try {
            await updateAgentTaskStatus({
                variables: {
                    id: agentTask.id,
                    targetStatus: AgentTaskStatus.REJECTED,
                },
            });
            toast.success('Task rejected');
            setRejectDialogOpen(false);
            setRejectFeedback('');
            refetch();
        } catch {
            toast.error('Failed to update status');
        }
    };

    const handleRetry = () => transitionTo(AgentTaskStatus.READY);

    const handleCopyErrors = () => {
        if (!agentTask?.errors) return;
        navigator.clipboard.writeText(agentTask.errors).then(() => {
            setErrorsCopied(true);
            setTimeout(() => setErrorsCopied(false), 2000);
        });
    };

    const renderStatusIcon = (status: string | undefined) => {
        const Icon = getStatusIcon(status, 'agentTask');
        return Icon ? <Icon className="mr-1 h-3 w-3" /> : null;
    };

    const currentStatus = agentTask?.status ?? '';
    const allowedTransitions = VALID_TRANSITIONS[currentStatus] ?? [];
    const canApprove = currentStatus === AgentTaskStatus.NEEDS_REVIEW;
    const canReject = currentStatus === AgentTaskStatus.NEEDS_REVIEW;
    const canRetry = currentStatus === AgentTaskStatus.FAILED;
    const repoUrl = agentTask?.project?.repository;
    const commitUrl =
        repoUrl && agentTask?.commitHash
            ? `${repoUrl.replace(/\/+$/, '')}/commit/${agentTask.commitHash}`
            : null;

    if (loading) {
        return (
            <div className="space-y-6">
                <DetailLayout
                    breadcrumbs={[{ label: 'Agent Tasks', to: '/agent-tasks' }, { label: 'Loading...' }]}
                    title="Loading..."
                />
                <LoadingState cards={1} rows={3} />
            </div>
        );
    }

    if (error || !agentTask) {
        return (
            <DetailLayout
                breadcrumbs={[{ label: 'Agent Tasks', to: '/agent-tasks' }, { label: 'Error' }]}
                title="Agent Task"
            >
                <ErrorState
                    message={error?.message ?? 'Agent task not found'}
                    onRetry={() => refetch()}
                />
            </DetailLayout>
        );
    }

    return (
        <DetailLayout
            breadcrumbs={[
                { label: 'Agent Tasks', to: '/agent-tasks' },
                { label: agentTask.title ?? 'Agent Task' },
            ]}
            title={agentTask.title ?? 'Agent Task'}
            typeLabel="Agent Task"
            statusNode={
                <Badge className={`${getStatusColor(agentTask.status ?? undefined, AGENT_TASK_STATUS_COLORS)} ${getStatusTextColor(agentTask.status ?? undefined, AGENT_TASK_STATUS_TEXT_COLORS)}`}>
                    {renderStatusIcon(agentTask.status ?? undefined)}
                    {agentTask.status}
                </Badge>
            }
            actions={
                <>
                    {canApprove && (
                        <Button onClick={handleApprove} disabled={transitionLoading}>
                            <CheckCircle className="h-4 w-4 mr-2" />
                            Approve
                        </Button>
                    )}
                    {canReject && (
                        <Button
                            variant="outline"
                            onClick={() => setRejectDialogOpen(true)}
                            disabled={transitionLoading}
                        >
                            <XCircle className="h-4 w-4 mr-2" />
                            Reject
                        </Button>
                    )}
                    {canRetry && (
                        <Button onClick={handleRetry} disabled={transitionLoading}>
                            <RotateCcw className="h-4 w-4 mr-2" />
                            Retry
                        </Button>
                    )}
                    <Button variant="outline" onClick={() => navigate('/agent-tasks')}>
                        Back to List
                    </Button>
                    <Button onClick={() => setUpdateDialogOpen(true)}>Edit</Button>
                    <Button variant="destructive" onClick={() => setConfirmDeleteOpen(true)} disabled={deleting}>
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
                                    {Object.values(AgentTaskStatus).map((status) => {
                                        const isAllowed = allowedTransitions.includes(
                                            status as AgentTaskStatus
                                        );
                                        return (
                                            <SelectItem
                                                key={status}
                                                value={status}
                                                className={!isAllowed ? 'opacity-50' : ''}
                                            >
                                                {status.replace(/_/g, ' ')}
                                                {!isAllowed ? ' (not allowed)' : ''}
                                            </SelectItem>
                                        );
                                    })}
                                </SelectContent>
                            </Select>
                        </div>
                        <Button
                            onClick={handleStatusChange}
                            disabled={
                                !selectedStatus ||
                                transitionLoading ||
                                !allowedTransitions.includes(
                                    selectedStatus as AgentTaskStatus
                                )
                            }
                        >
                            {transitionLoading ? 'Updating...' : 'Update Status'}
                        </Button>
                    </div>
                </CardContent>
            </Card>

            {agentTask.status === 'IN_PROGRESS' && (
                <Card>
                    <CardContent className="pt-6">
                        <div className="flex items-center gap-3">
                            <Progress value={50} className="flex-1" />
                            <span className="text-sm text-muted-foreground whitespace-nowrap">In Progress</span>
                        </div>
                    </CardContent>
                </Card>
            )}

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
                            <MarkdownViewer
                                content={agentTask.description}
                                className="prose prose-sm dark:prose-invert max-w-none"
                            />
                        </CardContent>
                    </Card>

                    {agentTask.result && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Result</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <MarkdownViewer
                                    content={agentTask.result}
                                    className="prose prose-sm dark:prose-invert max-w-none"
                                />
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Complexity Rating: {agentTask.complexityRating ?? '-'}</p>
                        <p>
                            Deliverable:{' '}
                            <Link
                                to={`/deliverables/${agentTask.deliverableId}`}
                                className="text-blue-600 hover:underline"
                            >
                                {agentTask.deliverableId ?? '-'}
                            </Link>
                        </p>
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
                                    {commitUrl ? (
                                        <a
                                            href={commitUrl}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="font-medium font-mono text-xs text-blue-600 hover:underline"
                                        >
                                            {agentTask.commitHash}
                                        </a>
                                    ) : (
                                        <p className="font-medium font-mono text-xs">
                                            {agentTask.commitHash || '-'}
                                        </p>
                                    )}
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
                                    <div className="p-4 bg-destructive/10 rounded-lg border border-destructive/20">
                                        <div className="flex items-center justify-between mb-2">
                                            <p className="text-sm font-medium text-destructive">
                                                Errors
                                            </p>
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                onClick={handleCopyErrors}
                                                className="h-7 text-xs"
                                            >
                                                {errorsCopied ? (
                                                    <Check className="h-3 w-3 mr-1" />
                                                ) : (
                                                    <Copy className="h-3 w-3 mr-1" />
                                                )}
                                                {errorsCopied ? 'Copied!' : 'Copy'}
                                            </Button>
                                        </div>
                                        <MarkdownViewer
                                            content={agentTask.errors}
                                            className="prose prose-sm dark:prose-invert max-w-none text-destructive"
                                        />
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
                onSuccess={() => {
                    toast.success('Agent task updated successfully');
                    refetch();
                }}
            />

            <Dialog open={rejectDialogOpen} onOpenChange={setRejectDialogOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Reject Agent Task</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 py-4">
                        <p className="text-sm text-muted-foreground">
                            Provide feedback for why this task is being rejected.
                        </p>
                        <Textarea
                            placeholder="Rejection feedback..."
                            value={rejectFeedback}
                            onChange={(e) => setRejectFeedback(e.target.value)}
                            rows={4}
                        />
                    </div>
                    <DialogFooter>
                        <Button
                            variant="outline"
                            onClick={() => setRejectDialogOpen(false)}
                        >
                            Cancel
                        </Button>
                        <Button variant="destructive" onClick={handleReject}>
                            Reject Task
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            <ConfirmDialog
                open={confirmDeleteOpen}
                onOpenChange={setConfirmDeleteOpen}
                title="Delete Agent Task"
                description="Are you sure you want to delete this agent task? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />

            <ActivityTimeline events={[]} />
        </DetailLayout>
    );
}
