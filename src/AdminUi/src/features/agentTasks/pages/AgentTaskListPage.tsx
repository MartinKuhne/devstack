import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
    PageHeader,
    FilterBar,
    DataPanel,
    LoadingState,
    ErrorState,
    EmptyState,
} from '@/components/layout';
import { Button } from '@/components/ui/button';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Trash2 } from 'lucide-react';
import { useAgentTasks } from '../hooks/useAgentTasks';
import { useDeleteAgentTaskMutation, type AgentTaskStatus } from '@/generated/graphql';
import { toast } from 'react-toastify';
import { AGENT_TASK_STATUS_COLORS, getStatusColor } from '@/lib/constants';
import { createModuleLogger } from '@/lib/logging';
import { ConfirmDialog } from '@/components/ConfirmDialog';

const logger = createModuleLogger('AgentTaskListPage');

const STATUS_FILTER_OPTIONS = [
    { value: 'all', label: 'All Statuses' },
    { value: 'READY', label: 'Ready' },
    { value: 'IN_PROGRESS', label: 'In Progress' },
    { value: 'NEEDS_REVIEW', label: 'Needs Review' },
    { value: 'DONE', label: 'Done' },
    { value: 'FAILED', label: 'Failed' },
    { value: 'REJECTED', label: 'Rejected' },
];

export function AgentTaskListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const [deleteAgentTask, { loading: deleting }] = useDeleteAgentTaskMutation();
    const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null);

    const statusFilter = searchParams.get('status') as AgentTaskStatus | undefined;
    const searchFilter = searchParams.get('search') || undefined;

    const [localSearch, setLocalSearch] = useState(searchFilter || '');
    const { agentTasks, loading, error, refetch } = useAgentTasks(
        undefined,
        statusFilter ? [statusFilter] : undefined
    );

    const handleDelete = async () => {
        if (!deleteTargetId) return;
        logger.info('Deleting agent task', { id: deleteTargetId });
        try {
            const result = await deleteAgentTask({
                variables: {
                    id: deleteTargetId,
                },
            });
            const deleted = result.data?.deleteAgentTask;
            if (!deleted) {
                logger.warn('Failed to delete agent task', { id: deleteTargetId });
                toast.error('Failed to delete agent task');
            } else {
                logger.info('Agent task deleted successfully', { id: deleteTargetId });
                toast.success('Agent task deleted successfully');
                refetch();
            }
        } catch {
            logger.error('Failed to delete agent task', { id: deleteTargetId });
            toast.error('Failed to delete agent task');
        }
        setDeleteTargetId(null);
    };

    const handleStatusChange = useCallback(
        (value: string) => {
            const newParams = new URLSearchParams(searchParams);
            if (value === 'all') {
                newParams.delete('status');
            } else {
                newParams.set('status', value);
            }
            setSearchParams(newParams);
        },
        [searchParams, setSearchParams]
    );

    const handleSearchChange = useCallback((value: string) => {
        setLocalSearch(value);
    }, []);

    const handleSearchSubmit = useCallback(() => {
        const newParams = new URLSearchParams(searchParams);
        if (localSearch) {
            newParams.set('search', localSearch);
        } else {
            newParams.delete('search');
        }
        setSearchParams(newParams);
    }, [localSearch, searchParams, setSearchParams]);

    const handleSearchClear = useCallback(() => {
        setLocalSearch('');
    }, []);

    const handleRowClick = (id: string | null | undefined) => {
        if (id) navigate(`/agent-tasks/${id}`);
    };

    const handleRowKeyDown = (e: React.KeyboardEvent, id: string | null | undefined) => {
        if ((e.key === 'Enter' || e.key === ' ') && id) {
            e.preventDefault();
            navigate(`/agent-tasks/${id}`);
        }
    };

    const filteredTasks = agentTasks.filter(
        (task): task is NonNullable<typeof task> =>
            task !== null &&
            (!searchFilter ||
                !!task.title?.toLowerCase().includes(searchFilter.toLowerCase()) ||
                !!task.deliverableId?.toLowerCase().includes(searchFilter.toLowerCase()))
    );

    return (
        <div className="space-y-6">
            <PageHeader
                title="Agent Tasks"
                description="Manage agent task execution and telemetry."
            />

            <FilterBar
                searchValue={localSearch}
                onSearchChange={handleSearchChange}
                onSearchSubmit={handleSearchSubmit}
                onSearchClear={handleSearchClear}
                selects={[
                    {
                        value: statusFilter || 'all',
                        onChange: handleStatusChange,
                        options: STATUS_FILTER_OPTIONS,
                    },
                ]}
            />

            <DataPanel title="Agent Task List">
                {error ? (
                    <ErrorState
                        message={error.message}
                        onRetry={() => refetch()}
                    />
                ) : loading ? (
                    <LoadingState rows={3} />
                ) : filteredTasks.length > 0 ? (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Title</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead>Agent</TableHead>
                                <TableHead>Tokens</TableHead>
                                <TableHead className="w-16"></TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {filteredTasks.map((task) => (
                                <TableRow
                                    key={task.id ?? ''}
                                    className="cursor-pointer hover:bg-muted/50"
                                    tabIndex={0}
                                    role="button"
                                    onClick={() => handleRowClick(task.id)}
                                    onKeyDown={(e) => handleRowKeyDown(e, task.id)}
                                >
                                    <TableCell className="font-medium">{task.title}</TableCell>
                                    <TableCell>
                                        <Badge
                                            className={getStatusColor(
                                                task.status ?? undefined,
                                                AGENT_TASK_STATUS_COLORS
                                            )}
                                        >
                                            {task.status}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>{task.agent || '-'}</TableCell>
                                    <TableCell>
                                        {(task.promptTokens ?? 0) +
                                            (task.completionTokens ?? 0)}
                                    </TableCell>
                                    <TableCell onClick={(e) => e.stopPropagation()}>
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-8 w-8 text-destructive hover:text-destructive"
                                            onClick={() => task.id && setDeleteTargetId(task.id)}
                                            disabled={deleting}
                                            aria-label={`Delete agent task ${task.title}`}
                                        >
                                            <Trash2 className="h-4 w-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                ) : (
                    <EmptyState description="No agent tasks found." />
                )}
            </DataPanel>

            <ConfirmDialog
                open={!!deleteTargetId}
                onOpenChange={(open) => !open && setDeleteTargetId(null)}
                title="Delete Agent Task"
                description="Are you sure you want to delete this agent task? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />
        </div>
    );
}
