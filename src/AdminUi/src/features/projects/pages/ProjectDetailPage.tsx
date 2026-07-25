import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useProjectContext } from '@/contexts/ProjectContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
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
import { useDeleteProjectMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';
import { DELIVERABLE_STATUS_COLORS, DELIVERABLE_STATUS_TEXT_COLORS, getStatusColor, getStatusTextColor, getStatusIcon } from '@/lib/constants';
import { createModuleLogger } from '@/lib/logging';
import { LoadingState, ErrorState, EmptyState, DetailLayout, ActivityTimeline } from '@/components/layout';
import DOMPurify from 'dompurify';
import { ConfirmDialog } from '@/components/ConfirmDialog';

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
    const { setProjectId } = useProjectContext();

    useEffect(() => {
        if (id) {
            setProjectId(id);
        }
    }, [id, setProjectId]);

    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [createDeliverableDialogOpen, setCreateDeliverableDialogOpen] = useState(false);
    const [deleteProject, { loading: deleting }] = useDeleteProjectMutation();
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

    const handleDelete = async () => {
        if (!project?.id) return;
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
        setConfirmDeleteOpen(false);
    };

    const handleRowKeyDown = (e: React.KeyboardEvent, path: string) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            navigate(path);
        }
    };

    const renderStatusIcon = (status: string | undefined) => {
        const Icon = getStatusIcon(status, 'deliverable');
        return Icon ? <Icon className="mr-1 h-3 w-3" /> : null;
    };

    if (loading) {
        return (
            <div className="space-y-6">
                <DetailLayout
                    breadcrumbs={[{ label: 'Projects', to: '/projects' }, { label: 'Loading...' }]}
                    title="Loading..."
                />
                <LoadingState cards={1} rows={3} />
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
            <DetailLayout
                breadcrumbs={[{ label: 'Projects', to: '/projects' }, { label: 'Error' }]}
                title="Project"
            >
                <ErrorState
                    message={error?.message ?? 'Project not found'}
                    onRetry={() => refetch()}
                />
            </DetailLayout>
        );
    }

    return (
        <DetailLayout
            breadcrumbs={[{ label: 'Projects', to: '/projects' }, { label: project.name }]}
            title={project.name}
            typeLabel="Project"
            statusNode={(() => {
                if (!project.repository) return undefined;
                const trimmed = project.repository.trim();
                const safeRepo = trimmed && !trimmed.startsWith('//') ? DOMPurify.sanitize(trimmed) : '';
                return safeRepo ? (
                    <a
                        href={safeRepo}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-sm text-blue-600 hover:underline"
                    >
                        {project.repository}
                    </a>
                ) : (
                    <span className="text-sm text-muted-foreground">{project.repository}</span>
                );
            })()}
            actions={
                <>
                    <Button variant="outline" onClick={() => navigate('/projects')}>
                        Back to Projects
                    </Button>
                    <Button onClick={() => setEditDialogOpen(true)}>Edit</Button>
                    <Button variant="destructive" onClick={() => setConfirmDeleteOpen(true)} disabled={deleting}>
                        Delete
                    </Button>
                </>
            }
        >
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
                        <ErrorState
                            message={deliverablesError.message}
                            onRetry={() => refetchDeliverables()}
                        />
                    ) : deliverablesLoading ? (
                        <LoadingState rows={3} />
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
                                            tabIndex={0}
                                            role="button"
                                            onClick={() =>
                                                deliverable.id &&
                                                navigate(`/deliverables/${deliverable.id}`)
                                            }
                                            onKeyDown={(e) =>
                                                deliverable.id &&
                                                handleRowKeyDown(e, `/deliverables/${deliverable.id}`)
                                            }
                                        >
                                            <TableCell className="font-medium">
                                                {deliverable.title}
                                            </TableCell>
                                            <TableCell>{deliverable.type}</TableCell>
                                            <TableCell>
                                                <Badge
                                                    className={`${getStatusColor(
                                                        deliverable.status ?? undefined,
                                                        DELIVERABLE_STATUS_COLORS
                                                    )} ${getStatusTextColor(
                                                        deliverable.status ?? undefined,
                                                        DELIVERABLE_STATUS_TEXT_COLORS
                                                    )}`}
                                                >
                                                    {renderStatusIcon(deliverable.status ?? undefined)}
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
                        <EmptyState
                            description="No deliverables yet."
                            action={{ label: 'New Deliverable', onClick: () => setCreateDeliverableDialogOpen(true) }}
                        />
                    )}
                </CardContent>
            </Card>
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
                onSuccess={() => {
                    toast.success('Project updated successfully');
                    refetch();
                }}
            />
            <CreateDeliverableDialog
                open={createDeliverableDialogOpen}
                onOpenChange={setCreateDeliverableDialogOpen}
                projectId={project?.id ?? ''}
                onSuccess={() => {
                    toast.success('Deliverable created successfully');
                    refetchDeliverables();
                }}
            />
            <ConfirmDialog
                open={confirmDeleteOpen}
                onOpenChange={setConfirmDeleteOpen}
                title="Delete Project"
                description="Are you sure you want to delete this project? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />

            <ActivityTimeline events={[]} />
        </DetailLayout>
    );
}
