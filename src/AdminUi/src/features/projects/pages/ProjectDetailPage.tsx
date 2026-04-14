import { useParams, useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useProject } from '@/features/projects/hooks/useProject';
import { EditProjectDialog } from '@/features/projects/components/EditProjectDialog';
import type { GetProjectQuery } from '@/generated/graphql';

export function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { project, loading, error, refetch } = useProject(id ?? '');
    const [editDialogOpen, setEditDialogOpen] = useState(false);

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

    const projectData = project as GetProjectQuery['projectById'];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">{projectData.name}</h2>
                    {projectData.githubUrl && (
                        <a
                            href={projectData.githubUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-sm text-blue-600 hover:underline"
                        >
                            {projectData.githubUrl}
                        </a>
                    )}
                </div>
                <Button variant="outline" onClick={() => navigate('/projects')}>
                    Back to Projects
                </Button>
                <Button onClick={() => setEditDialogOpen(true)}>
                    Edit
                </Button>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="features">Features</TabsTrigger>
                    <TabsTrigger value="defects">Defects</TabsTrigger>
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
                                {projectData.description ?? 'No description provided.'}
                            </p>
                        </CardContent>
                    </Card>

                    {projectData.architecture && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Architecture</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{projectData.architecture}</p>
                            </CardContent>
                        </Card>
                    )}

                    {projectData.memory && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Memory</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{projectData.memory}</p>
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Created: {projectData.createdAt ? new Date(projectData.createdAt).toLocaleString() : '-'}</p>
                        <p>Updated: {projectData.updatedAt ? new Date(projectData.updatedAt).toLocaleString() : '-'}</p>
                    </div>
                </TabsContent>

                <TabsContent value="features">
                    <Card>
                        <CardHeader>
                            <CardTitle>Features</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-muted-foreground text-sm">No features yet.</p>
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

                <TabsContent value="models">
                    <Card>
                        <CardHeader>
                            <CardTitle>Models</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-muted-foreground text-sm">No models configured.</p>
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="settings">
                    <Card>
                        <CardHeader>
                            <CardTitle>Settings</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-muted-foreground text-sm">Settings coming soon.</p>
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>
            <EditProjectDialog
                open={editDialogOpen}
                onOpenChange={setEditDialogOpen}
                project={projectData}
                onSuccess={() => refetch()}
                onError={(error) => {
                    if (error.includes('deleted')) {
                        navigate('/projects');
                    }
                }}
            />
        </div>
    );
}
