import { useNavigate } from 'react-router';
import { useState, useCallback, useMemo } from 'react';
import {
    PageHeader,
    DataPanel,
    LoadingState,
    ErrorState,
    EmptyState,
} from '@/components/layout';
import { Button } from '@/components/ui/button';
import { ChevronUp, ChevronDown } from 'lucide-react';
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
import { Pagination } from '@/components/ui/pagination';
import DOMPurify from 'dompurify';

const logger = createModuleLogger('ProjectListPage');

export function ProjectListPage() {
    const navigate = useNavigate();
    const { projects, loading, error, refetch } = useProjects();
    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const [sortField, setSortField] = useState<'name' | 'description' | 'repository'>('name');
    const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
    const pageSize = 25;

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

    const sortedProjects = useMemo(() => {
        if (!projects) return [];
        return [...projects].filter((p): p is NonNullable<typeof p> => p !== null).sort((a, b) => {
            const aVal = (a[sortField] ?? '').toString().toLowerCase();
            const bVal = (b[sortField] ?? '').toString().toLowerCase();
            if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
            if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
            return 0;
        });
    }, [projects, sortField, sortDirection]);

    const totalPages = Math.ceil(sortedProjects.length / pageSize);
    const paginatedProjects = sortedProjects.slice(
        (currentPage - 1) * pageSize,
        currentPage * pageSize
    );

    const handleSort = (field: 'name' | 'description' | 'repository') => {
        if (sortField === field) {
            setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDirection('asc');
        }
        setCurrentPage(1);
    };

    const renderSortIcon = (field: 'name' | 'description' | 'repository') => {
        if (sortField !== field) return null;
        return sortDirection === 'asc' ? <ChevronUp className="h-3 w-3 ml-1" /> : <ChevronDown className="h-3 w-3 ml-1" />;
    };

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
                    <>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('name')}>
                                        Name {renderSortIcon('name')}
                                    </button>
                                </TableHead>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('description')}>
                                        Description {renderSortIcon('description')}
                                    </button>
                                </TableHead>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('repository')}>
                                        Repository {renderSortIcon('repository')}
                                    </button>
                                </TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {paginatedProjects.map((project) => project ? (
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
                                            (() => {
                                                const trimmed = project.repository.trim();
                                                const safeRepo = trimmed && !trimmed.startsWith('//') ? DOMPurify.sanitize(trimmed) : '';
                                                return safeRepo ? (
                                                    <a
                                                        href={safeRepo}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="text-blue-600 hover:underline"
                                                        onClick={(e) => e.stopPropagation()}
                                                    >
                                                        {project.repository}
                                                    </a>
                                                ) : (
                                                    <span>{project.repository}</span>
                                                );
                                            })()
                                        ) : (
                                            '-'
                                        )}
                                    </TableCell>
                                </TableRow>
                            ) : null)}
                        </TableBody>
                    </Table>
                    <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
                    </>
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
