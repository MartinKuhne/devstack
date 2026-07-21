import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useProject as useProjectContext } from '@/contexts/ProjectContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { useProject } from '@/features/projects/hooks/useProject';
import { EditProjectDialog } from '@/features/projects/components/EditProjectDialog';
import { useDeliverables } from '@/features/deliverables/hooks/useDeliverables';
import { CreateDeliverableDialog } from '@/features/deliverables/components/CreateDeliverableDialog';
import { useAgentTasks } from '@/features/agentTasks/hooks/useAgentTasks';
import { CreateAgentTaskDialog } from '@/features/agentTasks/components/CreateAgentTaskDialog';
import { useDeleteProjectMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';
import { PROJECT_STATUS_COLORS, AGENT_TASK_STATUS_COLORS, getStatusColor } from '@/lib/constants';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('ProjectDetailPage');

export function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { project, loading, error, refetch } = useProject(id ?? '');
    const {
        deliverables,
        loading: deliverablesLoading,
        error: deliverablesError,
        refetch: refetchDeliverables,
    } = useDeliverables(id!);
    const {
        agentTasks,
        loading: agentTasksLoading,
        error: agentTasksError,
        refetch: refetchAgentTasks,
    } = useAgentTasks();
    const { setProjectId } = useProjectContext();

    useEffect(() => {
        if (id) {
            setProjectId(id);
        }
    }, [id, setProjectId]);

    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [createDeliverableDialogOpen, setCreateDeliverableDialogOpen] = useState(false);
    const [createAgentTaskDialogOpen, setCreateAgentTaskDialogOpen] = useState(false);
    const [deleteProject, { loading: deleting }] = useDeleteProjectMutation();

    const handleDelete = async () => {
        if (!project?.id) return;
        if (!confirm('Are you sure you want to delete this project? This action cannot be undone.'))
            return;
        logger.info('Deleting project', { id: project.id, name: project.name });
        try {
            const result = await deleteProject({
                variables: {
                    id: project.id,
                },
            });
            const deleted = result.data?.deleteProject;
            if (!deleted) {
                logger.warn('Failed to delete project', { id: project.id });
                toast.error('Failed to delete project');
            } else {
                logger.info('Project deleted successfully', { id: project.id });
                toast.success('Project deleted successfully');
                navigate('/projects');
            }
        } catch {
            logger.error('Failed to delete project', { id: project.id });
            toast.error('Failed to delete project');
        }
    };

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <Skeleton className="h-8 w-48" />
                    <Skeleton className="h-4 w-32 mt-2" />
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Project Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="space-y-2">
                            <Skeleton className="h-4 w-32" />
                            <Skeleton className="h-8 w-full" />
                        </div>
                        <div className="space-y-2">
                            <Skeleton className="h-4 w-32" />
                            <Skeleton className="h-24 w-full" />
                        </div>
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !project) {
        if (error) {
            logger.error('Failed to load project', {
                id,
                message: error.message,
            });
        }
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Project</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading project</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <p className="text-sm text-destructive">
                            {error?.message ?? 'Project not found'}
                        </p>
                        <p className="text-sm text-muted-foreground">
                            This error has been logged. Please try refreshing the page or contact
                            support if the issue persists.
                        </p>
                        <div className="flex gap-2">
                            <Button variant="outline" onClick={() => refetch()}>
                                Retry
                            </Button>
                            <Button
                                variant="outline"
                                className="mt-4"
                                onClick={() => navigate('/projects')}
                            >
                                Back to Projects
                            </Button>
                        </div>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">{project.name}</h2>
                    {project.repository && (
                        <a
                            href={project.repository}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-sm text-blue-600 hover:underline"
                        >
                            {project.repository}
                        </a>
                    )}
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate('/projects')}>
                        Back to Projects
                    </Button>
                    <Button onClick={() => setEditDialogOpen(true)}>Edit</Button>
                    <Button variant="destructive" onClick={handleDelete} disabled={deleting}>
                        Delete
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="deliverables" className="w-full">
                <TabsList>
                    <TabsTrigger value="deliverables">Deliverables</TabsTrigger>
                    <TabsTrigger value="agent-tasks">Agent Tasks</TabsTrigger>
                </TabsList>

                <TabsContent value="deliverables">
                    <Card>
                        <CardHeader>
                            <div className="flex items-center justify-between">
                                <CardTitle>Deliverables</CardTitle>
                                <Button onClick={() => setCreateDeliverableDialogOpen(true)}>
                                    New Deliverable
                                </Button>
                            </div>
                        </CardHeader>
                        <CardContent>
                            {deliverablesError ? (
                                <p className="text-sm text-destructive">
                                    {deliverablesError.message}
                                </p>
                            ) : deliverablesLoading ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Type</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>ID</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[1, 2, 3].map((item) => (
                                            <TableRow key={item}>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-64" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-20" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-6 w-20" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-24" />
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : deliverables && deliverables.length > 0 ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Type</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>ID</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {deliverables.map((deliverable) =>
                                            deliverable ? (
                                                <TableRow
                                                    key={deliverable.id ?? ''}
                                                    className="cursor-pointer hover:bg-muted/50"
                                                    onClick={() =>
                                                        deliverable.id &&
                                                        navigate(`/deliverables/${deliverable.id}`)
                                                    }
                                                >
                                                    <TableCell className="font-medium">
                                                        {deliverable.title}
                                                    </TableCell>
                                                    <TableCell>{deliverable.type}</TableCell>
                                                    <TableCell>
                                                        <Badge
                                                            className={getStatusColor(
                                                                deliverable.status ?? undefined,
                                                                PROJECT_STATUS_COLORS
                                                            )}
                                                        >
                                                            {deliverable.status}
                                                        </Badge>
                                                    </TableCell>
                                                    <TableCell className="text-xs font-mono">
                                                        {deliverable.id ?? '-'}
                                                    </TableCell>
                                                </TableRow>
                                            ) : null
                                        )}
                                    </TableBody>
                                </Table>
                            ) : (
                                <p className="text-muted-foreground text-sm">
                                    No deliverables yet.
                                </p>
                            )}
                        </CardContent>
                    </Card>
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
                                <p className="text-sm text-destructive">
                                    {agentTasksError.message}
                                </p>
                            ) : agentTasksLoading ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Agent</TableHead>
                                            <TableHead>Tokens</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[1, 2, 3].map((item) => (
                                            <TableRow key={item}>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-64" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-6 w-20" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-24" />
                                                </TableCell>
                                                <TableCell>
                                                    <Skeleton className="h-4 w-24" />
                                                </TableCell>
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
                                            <TableHead>Agent</TableHead>
                                            <TableHead>Tokens</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {agentTasks.map((task) =>
                                            task ? (
                                                <TableRow
                                                    key={task.id ?? ''}
                                                    className="cursor-pointer hover:bg-muted/50"
                                                    onClick={() =>
                                                        task.id &&
                                                        navigate(`/agent-tasks/${task.id}`)
                                                    }
                                                >
                                                    <TableCell className="font-medium">
                                                        {task.title}
                                                    </TableCell>
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
                                                </TableRow>
                                            ) : null
                                        )}
                                    </TableBody>
                                </Table>
                            ) : (
                                <p className="text-muted-foreground text-sm">No agent tasks yet.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

            </Tabs>
            <EditProjectDialog
                open={editDialogOpen}
                onOpenChange={setEditDialogOpen}
                project={
                    project
                        ? {
                              id: project.id ?? '',
                              name: project.name ?? '',
                              description: project.description ?? '',
                              repository: project.repository ?? '',
                          }
                        : null
                }
                onSuccess={() => refetch()}
            />
            <CreateDeliverableDialog
                open={createDeliverableDialogOpen}
                onOpenChange={setCreateDeliverableDialogOpen}
                projectId={project?.id ?? ''}
                onSuccess={() => {
                    refetchDeliverables();
                }}
            />
            <CreateAgentTaskDialog
                open={createAgentTaskDialogOpen}
                onOpenChange={setCreateAgentTaskDialogOpen}
                deliverableId={id ?? ''}
                projectId={project?.id ?? ''}
                onSuccess={() => {
                    refetchAgentTasks();
                }}
            />
        </div>
    );
}
