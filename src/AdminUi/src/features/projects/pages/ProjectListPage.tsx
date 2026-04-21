import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { CreateProjectDialog } from '@/features/projects/components/CreateProjectDialog';

export function ProjectListPage() {
    const navigate = useNavigate();
    const { projects, loading, error, refetch } = useProjects();
    const [createDialogOpen, setCreateDialogOpen] = useState(false);

    const handleRowClick = (id: string) => {
        navigate(`/projects/${id}`);
    };

    const handleSuccess = useCallback(() => {
        refetch();
    }, [refetch]);

    if (loading) {
        return (
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h2 className="text-2xl font-bold tracking-tight">Projects</h2>
                        <p className="text-muted-foreground">Manage your development projects.</p>
                    </div>
                    <Button>New Project</Button>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Project List</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Name</TableHead>
                                    <TableHead>Description</TableHead>
                                    <TableHead>Repository</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {[1, 2, 3].map((item) => (
                                    <TableRow key={item}>
                                        <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                                        <TableCell><Skeleton className="h-4 w-48" /></TableCell>
                                        <TableCell><Skeleton className="h-4 w-40" /></TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error) {
        return (
            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h2 className="text-2xl font-bold tracking-tight">Projects</h2>
                        <p className="text-muted-foreground">Manage your development projects.</p>
                    </div>
                    <Button>New Project</Button>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading projects</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error.message}</p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Projects</h2>
                    <p className="text-muted-foreground">Manage your development projects.</p>
                </div>
                <Button onClick={() => setCreateDialogOpen(true)}>New Project</Button>
            </div>
            <Card>
                <CardHeader>
                    <CardTitle>Project List</CardTitle>
                </CardHeader>
                <CardContent>
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
                                {projects.map((project: { id: string | null; name: string | null; description: string | null; repository: string | null }) => (
                                    <TableRow
                                        key={project.id ?? ''}
                                        className="cursor-pointer hover:bg-muted/50"
                                        onClick={() => handleRowClick(project.id ?? '')}
                                    >
                                        <TableCell className="font-medium">{project.name}</TableCell>
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
                                            ) : '-'}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    ) : (
                        <div className="py-8 text-center">
                            <p className="text-muted-foreground">No projects yet. Create your first project to get started.</p>
                        </div>
                    )}
                </CardContent>
            </Card>
            <CreateProjectDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                onSuccess={handleSuccess}
            />
        </div>
    );
}
