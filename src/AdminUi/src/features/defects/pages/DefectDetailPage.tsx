import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useQuery } from '@apollo/client/react';
import { gql } from '@apollo/client/core';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { GetDefectByIdQuery } from '@/generated/graphql';
import { toast } from 'react-toastify';
import { EditDefectDialog } from '../components/EditDefectDialog';

const GET_DEFECT = gql`
    query GetDefectById($id: ID!) {
        defectById(id: $id) {
            id
            title
            description
            acceptanceCriteria
            plan
            result
            errors
            securityImpact
            performanceImpact
            status
            severity
            parentFeature {
                id
                title
            }
            project {
                id
                name
            }
            createdAt
            updatedAt
            version
        }
    }
`;

const SEVERITY_COLORS: Record<string, string> = {
    Critical: 'bg-red-600',
    High: 'bg-red-500',
    Medium: 'bg-yellow-500',
    Low: 'bg-green-500',
};

const STATUS_COLORS: Record<string, string> = {
    Reported: 'bg-gray-500',
    Triaged: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Resolved: 'bg-purple-500',
    Closed: 'bg-green-500',
};

function MarkdownSection({ title, content }: { title: string; content?: string | null }) {
    if (!content) return null;
    
    return (
        <Card>
            <CardHeader>
                <CardTitle>{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <div className="prose prose-sm max-w-none whitespace-pre-wrap">
                    {content}
                </div>
            </CardContent>
        </Card>
    );
}

export function DefectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
    const { data, loading, error, refetch } = useQuery<GetDefectByIdQuery>(GET_DEFECT, {
        client: getApolloClient(),
        variables: { id: id ?? '' },
        skip: !id,
        fetchPolicy: 'cache-and-network',
    });

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <Skeleton className="h-8 w-48" />
                    <Skeleton className="h-4 w-32 mt-2" />
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Defect Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-24 w-full" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !data?.defectById) {
        toast.error('Defect not found');
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Defect</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading defect</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">Defect not found</p>
                        <Button variant="outline" className="mt-4" onClick={() => navigate('/defects')}>
                            Back to Defects
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    const defect = data.defectById;

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <div className="flex items-center gap-3">
                        <h2 className="text-2xl font-bold tracking-tight">{defect.title}</h2>
                        <Badge className={STATUS_COLORS[defect.status] || 'bg-gray-500'}>
                            {defect.status}
                        </Badge>
                        <Badge className={SEVERITY_COLORS[defect.severity] || 'bg-gray-500'}>
                            {defect.severity}
                        </Badge>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate('/defects')}>
                        Back
                    </Button>
                    <Button onClick={() => setIsEditDialogOpen(true)}>
                        Edit
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <div className="md:col-span-2 space-y-4">
                            <MarkdownSection title="Description" content={defect.description} />
                            <MarkdownSection title="Acceptance Criteria" content={defect.acceptanceCriteria} />
                            <MarkdownSection title="Plan" content={defect.plan} />
                            <MarkdownSection title="Result" content={defect.result} />
                            <MarkdownSection title="Errors" content={defect.errors} />
                        </div>

                        <div className="space-y-4">
                            <Card>
                                <CardHeader>
                                    <CardTitle className="text-sm">Details</CardTitle>
                                </CardHeader>
                                <CardContent className="space-y-4">
                                    <div>
                                        <h4 className="text-xs font-medium text-muted-foreground mb-1">Status</h4>
                                        <Badge className={STATUS_COLORS[defect.status] || 'bg-gray-500'}>
                                            {defect.status}
                                        </Badge>
                                    </div>
                                    <div>
                                        <h4 className="text-xs font-medium text-muted-foreground mb-1">Severity</h4>
                                        <Badge className={SEVERITY_COLORS[defect.severity] || 'bg-gray-500'}>
                                            {defect.severity}
                                        </Badge>
                                    </div>
                                    {defect.parentFeature && (
                                        <div>
                                            <h4 className="text-xs font-medium text-muted-foreground mb-1">Parent Feature</h4>
                                            <Button
                                                variant="link"
                                                className="p-0 h-auto font-normal"
                                                onClick={() => navigate(`/features/${defect.parentFeature?.id}`)}
                                            >
                                                {defect.parentFeature?.title}
                                            </Button>
                                        </div>
                                    )}
                                    {defect.project && (
                                        <div>
                                            <h4 className="text-xs font-medium text-muted-foreground mb-1">Project</h4>
                                            <p className="text-sm">{defect.project.name}</p>
                                        </div>
                                    )}
                                    <div>
                                        <h4 className="text-xs font-medium text-muted-foreground mb-1">Created</h4>
                                        <p className="text-sm">{new Date(defect.createdAt).toLocaleDateString()}</p>
                                    </div>
                                    <div>
                                        <h4 className="text-xs font-medium text-muted-foreground mb-1">Updated</h4>
                                        <p className="text-sm">{new Date(defect.updatedAt).toLocaleDateString()}</p>
                                    </div>
                                </CardContent>
                            </Card>

                            <Card>
                                <CardHeader>
                                    <CardTitle className="text-sm">Impacts</CardTitle>
                                </CardHeader>
                                <CardContent className="space-y-4">
                                    <MarkdownSection title="Security Impact" content={defect.securityImpact} />
                                    <MarkdownSection title="Performance Impact" content={defect.performanceImpact} />
                                </CardContent>
                            </Card>
                        </div>
                    </div>
                </TabsContent>
            </Tabs>

            <EditDefectDialog
                open={isEditDialogOpen}
                onOpenChange={setIsEditDialogOpen}
                defect={defect ? {
                    id: defect.id,
                    title: defect.title,
                    description: defect.description,
                    acceptanceCriteria: defect.acceptanceCriteria,
                    plan: defect.plan,
                    securityImpact: defect.securityImpact,
                    performanceImpact: defect.performanceImpact,
                    severity: defect.severity,
                    updatedAt: defect.updatedAt,
                    version: defect.version ?? 1,
                } : null}
                onSuccess={() => {
                    refetch();
                }}
            />
        </div>
    );
}
