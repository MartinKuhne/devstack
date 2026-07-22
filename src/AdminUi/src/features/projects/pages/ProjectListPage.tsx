import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import {
    PageHeader,
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
import { useProjects } from '@/features/projects/hooks/useProjects';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';
import { createModuleLogger, formatGraphQLError } from '@/lib/logging';
import { toast } from 'react-toastify';

const logger = createModuleLogger('ProjectListPage');

export function ProjectListPage() {
    const navigate = useNavigate();
    const { projects, loading, error, refetch } = useProjects();
    const [createDialogOpen, setCreateDialogOpen] = useState(false);

    const handleRowClick = (id: string) => {
        logger.debug('Navigating to project', { id });
        navigate(`/projects/${id}`);
    };

    const handleRowKeyDown = (e: React.KeyboardEvent, id: string) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleRowClick(id);
        }
    };

    const handleSuccess = useCallback(() => {
        logger.info('Project created, refetching list');
        toast.success('Project created successfully');
        refetch();
    }, [refetch]);

    const handleCreateProject = useCallback(() => {
        setCreateDialogOpen(true);
    }, []);

    const handleRetry = useCallback(() => {
        refetch();
    }, [refetch]);

    if (loading) {
        return (
            <div className="space-y-6">
                <PageHeader title="Projects" description="Manage your development projects." actionSlot={<Button onClick={handleCreateProject}>New Project</Button>} />
                <DataPanel>
                    <LoadingState rows={3} />
                </DataPanel>
            </div>
        );
    }

    if (error) {
        const errorInfo = formatGraphQLError(error);
        logger.error('Failed to load projects', {
            message: errorInfo.message,
            details: errorInfo.details,
        });
        return (
            <div className="space-y-6">
                <PageHeader title="Projects" description="Manage your development projects." actionSlot={<Button onClick={handleRetry}>Retry</Button>} />
                <ErrorState message={error.message} onRetry={handleRetry} />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <PageHeader
                title="Projects"
                description="Manage your development projects."
                actionSlot={<Button onClick={handleCreateProject}>New Project</Button>}
            />
            <DataPanel title="Project List">
                {projects && projects.length > 0 ? (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Description</TableHead>
                                <TableHead>Repository</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {projects.map((project) => project ? (
                                <TableRow
                                    key={project.id ?? ''}
                                    className="cursor-pointer hover:bg-muted/50"
                                    tabIndex={0}
                                    role="button"
                                    onClick={() => handleRowClick(project.id ?? '')}
                                    onKeyDown={(e) => handleRowKeyDown(e, project.id ?? '')}
                                >
                                    <TableCell className="font-medium">
                                        {project.name}
                                    </TableCell>
                                    <TableCell className="max-w-xs truncate">
                                        {project.description ?? '-'}
                                    </TableCell>
                                    <TableCell>
                                        {project.repository ? (
                                            <a
                                                href={project.repository}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="text-blue-600 hover:underline"
                                                onClick={(e) => e.stopPropagation()}
                                            >
                                                {project.repository}
                                            </a>
                                        ) : (
                                            '-'
                                        )}
                                    </TableCell>
                                </TableRow>
                            ) : null)}
                        </TableBody>
                    </Table>
                ) : (
                    <EmptyState
                        description="No projects yet. Create your first project to get started."
                        action={{ label: 'Create Project', onClick: () => setCreateDialogOpen(true) }}
                    />
                )}
            </DataPanel>
            <CreateProjectDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                onSuccess={handleSuccess}
            />
        </div>
    );
}
