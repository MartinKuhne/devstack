import { useParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { useProject } from '@/features/projects/hooks/useProject';
import { EditProjectDialog } from '@/features/projects/components/EditProjectDialog';
import { LargeLanguageModelList } from '@/features/largeLanguageModels/components/LargeLanguageModelList';
import { LargeLanguageModelDialog } from '@/features/largeLanguageModels/components/LargeLanguageModelDialog';
import { GitHubConfigurationSection } from '@/features/projects/components/GitHubConfigurationSection';
import { CreateFeatureDialog } from '@/features/features/components/CreateFeatureDialog';
import { useFeatures } from '@/features/features/hooks/useFeatures';
import { useDeliverables } from '@/features/deliverables/hooks/useDeliverables';
import { CreateDeliverableDialog } from '@/features/deliverables/components/CreateDeliverableDialog';
import { useAgentTasks } from '@/features/agentTasks/hooks/useAgentTasks';
import { CreateAgentTaskDialog } from '@/features/agentTasks/components/CreateAgentTaskDialog';
import { useDeleteProjectMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';

const STATUS_COLORS: Record<string, string> = {
    Planned: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Review: 'bg-purple-500',
    Done: 'bg-green-500',
    Failed: 'bg-red-500',
};

export function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { project, loading, error, refetch } = useProject(id ?? '');
    const { features, loading: featuresLoading, error: featuresError, refetch: refetchFeatures } = useFeatures(id ?? '');
    const { deliverables, loading: deliverablesLoading, error: deliverablesError, refetch: refetchDeliverables } = useDeliverables(id ?? '');
    const { agentTasks, loading: agentTasksLoading, error: agentTasksError, refetch: refetchAgentTasks } = useAgentTasks(id ?? '');
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [addModelDialogOpen, setAddModelDialogOpen] = useState(false);
    const [createFeatureDialogOpen, setCreateFeatureDialogOpen] = useState(false);
    const [createDeliverableDialogOpen, setCreateDeliverableDialogOpen] = useState(false);
    const [createAgentTaskDialogOpen, setCreateAgentTaskDialogOpen] = useState(false);
    const [deleteProject, { loading: deleting }] = useDeleteProjectMutation();

    const handleDelete = async () => {
        if (!project?.id) return;
        if (!confirm('Are you sure you want to delete this project? This action cannot be undone.')) return;
        try {
            const result = await deleteProject({
                variables: {
                    input: { id: project.id },
                },
            });
            if (result.data?.deleteProject?.errors?.length) {
                toast.error(result.data.deleteProject.errors.join(', '));
            } else {
                toast.success('Project deleted successfully');
                navigate('/projects');
            }
        } catch {
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
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Project</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading project</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error?.message ?? 'Project not found'}</p>
                        <Button variant="outline" className="mt-4" onClick={() => navigate('/projects')}>
                            Back to Projects
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
                    <h2 className="text-2xl font-bold tracking-tight">{project.name}</h2>
                    {project.githubUrl && (
                        <a
                            href={project.githubUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-sm text-blue-600 hover:underline"
                        >
                            {project.githubUrl}
                        </a>
                    )}
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate('/projects')}>
                        Back to Projects
                    </Button>
                    <Button onClick={() => setEditDialogOpen(true)}>
                        Edit
                    </Button>
                    <Button variant="destructive" onClick={handleDelete} disabled={deleting}>
                        Delete
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="features">Features</TabsTrigger>
                    <TabsTrigger value="defects">Defects</TabsTrigger>
                    <TabsTrigger value="deliverables">Deliverables</TabsTrigger>
                    <TabsTrigger value="agent-tasks">Agent Tasks</TabsTrigger>
                    <TabsTrigger value="models">Models</TabsTrigger>
                    <TabsTrigger value="settings">Settings</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Description</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-sm whitespace-pre-wrap">
                                {project.description ?? 'No description provided.'}
                            </p>
                        </CardContent>
                    </Card>

                    {project.architecture && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Architecture</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{project.architecture}</p>
                            </CardContent>
                        </Card>
                    )}

                    {project.memory && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Memory</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{project.memory}</p>
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Created: {project.createdAt ? new Date(project.createdAt).toLocaleString() : '-'}</p>
                        <p>Updated: {project.updatedAt ? new Date(project.updatedAt).toLocaleString() : '-'}</p>
                    </div>
                </TabsContent>

                <TabsContent value="features">
                    <Card>
                        <CardHeader>
                            <div className="flex items-center justify-between">
                                <CardTitle>Features</CardTitle>
                                <Button onClick={() => setCreateFeatureDialogOpen(true)}>
                                    New Feature
                                </Button>
                            </div>
                        </CardHeader>
                        <CardContent>
                            {featuresError ? (
                                <p className="text-sm text-destructive">{featuresError.message}</p>
                            ) : featuresLoading ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Tasks</TableHead>
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[1, 2, 3].map((item) => (
                                            <TableRow key={item}>
                                                <TableCell><Skeleton className="h-4 w-64" /></TableCell>
                                                <TableCell><Skeleton className="h-6 w-20" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-8" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : features && features.length > 0 ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Tasks</TableHead>
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {features.map((feature) => (
                                            <TableRow
                                                key={feature.id ?? ''}
                                                className="cursor-pointer hover:bg-muted/50"
                                                onClick={() => feature.id && navigate(`/features/${feature.id}`)}
                                            >
                                                <TableCell className="font-medium">{feature.title}</TableCell>
                                                <TableCell>
                                                    <Badge className={STATUS_COLORS[feature.status ?? ''] || 'bg-gray-500'}>
                                                        {feature.status}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell>{feature.tasks?.length || 0}</TableCell>
                                                <TableCell>
                                                    {feature.updatedAt ? new Date(feature.updatedAt).toLocaleDateString() : '-'}
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : (
                                <p className="text-muted-foreground text-sm">No features yet.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="defects">
                    <Card>
                        <CardHeader>
                            <CardTitle>Defects</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-muted-foreground text-sm">No defects yet.</p>
                        </CardContent>
                    </Card>
                </TabsContent>

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
                                <p className="text-sm text-destructive">{deliverablesError.message}</p>
                            ) : deliverablesLoading ? (
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Title</TableHead>
                                            <TableHead>Type</TableHead>
                                            <TableHead>Status</TableHead>
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {[1, 2, 3].map((item) => (
                                            <TableRow key={item}>
                                                <TableCell><Skeleton className="h-4 w-64" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                                                <TableCell><Skeleton className="h-6 w-20" /></TableCell>
                                                <TableCell><Skeleton className="h-4 w-24" /></TableCell>
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
                                            <TableHead>Updated</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {deliverables.map((deliverable) => (
                                            <TableRow
                                                key={deliverable.id ?? ''}
                                                className="cursor-pointer hover:bg-muted/50"
                                                onClick={() => deliverable.id && navigate(`/deliverables/${deliverable.id}`)}
                                            >
                                                <TableCell className="font-medium">{deliverable.title}</TableCell>
                                                <TableCell>{deliverable.subtype}</TableCell>
                                                <TableCell>
                                                    <Badge className={STATUS_COLORS[deliverable.status ?? ''] || 'bg-gray-500'}>
                                                        {deliverable.status}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell>
                                                    {deliverable.updatedAt ? new Date(deliverable.updatedAt).toLocaleDateString() : '-'}
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            ) : (
                                <p className="text-muted-foreground text-sm">No deliverables yet.</p>
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
                                <p className="text-muted-foreground text-sm">No agent tasks yet.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="models">
                    <LargeLanguageModelList
                        onAddModel={() => setAddModelDialogOpen(true)}
                    />
                </TabsContent>

                <TabsContent value="settings">
                    <GitHubConfigurationSection project={project} onProjectUpdated={refetch} />
                </TabsContent>
            </Tabs>
            <EditProjectDialog
                open={editDialogOpen}
                onOpenChange={setEditDialogOpen}
                project={project ? {
                    id: project.id ?? '',
                    name: project.name ?? '',
                    description: project.description,
                    architecture: project.architecture,
                    memory: project.memory,
                    githubUrl: project.githubUrl,
                } : null}
                onSuccess={() => refetch()}
                onError={(error) => {
                    if (error.includes('deleted')) {
                        navigate('/projects');
                    }
                }}
            />
            <LargeLanguageModelDialog
                open={addModelDialogOpen}
                onOpenChange={setAddModelDialogOpen}
                onSuccess={() => refetch()}
            />
            <CreateFeatureDialog
                open={createFeatureDialogOpen}
                onOpenChange={setCreateFeatureDialogOpen}
                projectId={id ?? ''}
                onSuccess={(featureId) => {
                    refetchFeatures();
                    if (featureId) {
                        navigate(`/features/${featureId}`);
                    }
                }}
            />
            <CreateDeliverableDialog
                open={createDeliverableDialogOpen}
                onOpenChange={setCreateDeliverableDialogOpen}
                onSuccess={(deliverableId) => {
                    refetchDeliverables();
                    if (deliverableId) {
                        navigate(`/deliverables/${deliverableId}`);
                    }
                }}
            />
            <CreateAgentTaskDialog
                open={createAgentTaskDialogOpen}
                onOpenChange={setCreateAgentTaskDialogOpen}
                projectId={id ?? ''}
                onSuccess={() => {
                    refetchAgentTasks();
                }}
            />
        </div>
    );
}
