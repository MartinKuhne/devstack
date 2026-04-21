import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Trash2 } from 'lucide-react';
import { useAgentTasks } from '../hooks/useAgentTasks';
import { useDeleteAgentTaskMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';
import type { AgentTaskStatus } from '@/generated/graphql';
import { AGENT_TASK_STATUS_COLORS, getStatusColor } from '@/lib/constants';

export function AgentTaskListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const [deleteAgentTask, { loading: deleting }] = useDeleteAgentTaskMutation();
    
    const statusFilter = searchParams.get('status') as AgentTaskStatus | undefined;
    const searchFilter = searchParams.get('search') || undefined;
    
    const [localSearch, setLocalSearch] = useState(searchFilter || '');
    const { agentTasks, loading, error, refetch } = useAgentTasks(statusFilter ? [statusFilter] : undefined);

    const handleDelete = async (id: string) => {
        if (!confirm('Are you sure you want to delete this agent task?')) return;
        try {
            const result = await deleteAgentTask({
                variables: {
                    input: { id },
                },
            });
            if (result.data?.deleteAgentTask?.errors?.length) {
                toast.error(result.data.deleteAgentTask.errors.join(', '));
            } else {
                toast.success('Agent task deleted successfully');
                refetch();
            }
        } catch {
            toast.error('Failed to delete agent task');
        }
    };

    const handleStatusChange = useCallback((value: string) => {
        const newParams = new URLSearchParams(searchParams);
        if (value === 'all') {
            newParams.delete('status');
        } else {
            newParams.set('status', value);
        }
        setSearchParams(newParams);
    }, [searchParams, setSearchParams]);

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setLocalSearch(e.target.value);
    }, []);

    const handleSearchSubmit = useCallback((e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const newParams = new URLSearchParams(searchParams);
        if (localSearch) {
            newParams.set('search', localSearch);
        } else {
            newParams.delete('search');
        }
        setSearchParams(newParams);
    }, [localSearch, searchParams, setSearchParams]);

    const handleRowClick = (id: string | null | undefined) => {
        if (id) navigate(`/agent-tasks/${id}`);
    };

    const filteredTasks = agentTasks.filter(task => {
        if (!searchFilter) return true;
        const searchLower = searchFilter.toLowerCase();
        return (
            task.title?.toLowerCase().includes(searchLower) ||
            task.deliverableId?.toLowerCase().includes(searchLower)
        );
    });

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Agent Tasks</h2>
                    <p className="text-muted-foreground">Manage agent task execution and telemetry.</p>
                </div>
            </div>

            <div className="flex gap-4 items-end">
                <div className="w-48">
                    <Select value={statusFilter || 'all'} onValueChange={handleStatusChange}>
                        <SelectTrigger>
                            <SelectValue placeholder="Filter by status" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Statuses</SelectItem>
                            <SelectItem value="READY">Ready</SelectItem>
                            <SelectItem value="IN_PROGRESS">In Progress</SelectItem>
                            <SelectItem value="NEEDS_REVIEW">Needs Review</SelectItem>
                            <SelectItem value="DONE">Done</SelectItem>
                            <SelectItem value="FAILED">Failed</SelectItem>
                            <SelectItem value="REJECTED">Rejected</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <form onSubmit={handleSearchSubmit} className="flex-1 max-w-sm">
                    <div className="flex gap-2">
                        <Input
                            placeholder="Search agent tasks..."
                            value={localSearch}
                            onChange={handleSearchChange}
                        />
                        <Button type="submit" variant="secondary">Search</Button>
                    </div>
                </form>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Agent Task List</CardTitle>
                </CardHeader>
                <CardContent>
                    {error ? (
                        <p className="text-sm text-destructive">{error.message}</p>
                    ) : loading ? (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Agent</TableHead>
                                     <TableHead>Updated</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {[1, 2, 3].map((item) => (
                                    <TableRow key={item}>
                                        <TableCell><div className="h-4 w-64 bg-muted rounded" /></TableCell>
                                        <TableCell><div className="h-6 w-20 bg-muted rounded" /></TableCell>
                                        <TableCell><div className="h-4 w-24 bg-muted rounded" /></TableCell>
                                        <TableCell><div className="h-4 w-24 bg-muted rounded" /></TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    ) : filteredTasks.length > 0 ? (
                        <Table>
                          <TableHeader>
                                    <TableRow>
                                        <TableHead>Title</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>Agent</TableHead>
                                        <TableHead>Tokens</TableHead>
                                        <TableHead>Updated</TableHead>
                                        <TableHead className="w-16"></TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {filteredTasks.map((task) => (
                                        <TableRow
                                            key={task.id ?? ''}
                                            className="cursor-pointer hover:bg-muted/50"
                                            onClick={() => handleRowClick(task.id)}
                                        >
                                            <TableCell className="font-medium">{task.title}</TableCell>
                                           <TableCell>
                                                    <Badge className={getStatusColor(task.status ?? undefined, AGENT_TASK_STATUS_COLORS)}>
                                                        {task.status}
                                                    </Badge>
                                                </TableCell>
                                            <TableCell>{task.agent || '-'}</TableCell>
                                            <TableCell>
                                                {(task.promptTokens ?? 0) + (task.completionTokens ?? 0)}
                                            </TableCell>
                                            <TableCell>-</TableCell>
                                            <TableCell onClick={(e) => e.stopPropagation()}>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-8 w-8 text-destructive hover:text-destructive"
                                                    onClick={() => task.id && handleDelete(task.id)}
                                                    disabled={deleting}
                                                >
                                                    <Trash2 className="h-4 w-4" />
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                            </TableBody>
                        </Table>
                    ) : (
                        <p className="text-muted-foreground text-sm">No agent tasks found.</p>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
