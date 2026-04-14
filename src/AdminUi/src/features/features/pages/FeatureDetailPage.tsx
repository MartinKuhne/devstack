import { useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useFeature } from '@/features/features/hooks/useFeature';
import { EditFeatureDialog } from '@/features/features/components/EditFeatureDialog';
import { TaskBoard } from '@/features/features/components/TaskBoard';
import { toast } from 'react-toastify';

const STATUS_COLORS: Record<string, string> = {
    Planned: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Review: 'bg-purple-500',
    Done: 'bg-green-500',
    Failed: 'bg-red-500',
};

interface SimpleTask {
    id: string;
    title: string;
    status: string;
}

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

export function FeatureDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { data, loading, error, refetch } = useFeature(id ?? '');
    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);

    const handleEditOpen = useCallback(() => {
        setIsEditDialogOpen(true);
    }, []);

    const handleEditSuccess = useCallback(() => {
        toast.success('Feature updated successfully');
        refetch();
    }, [refetch]);

    const handleEditError = useCallback((errorMessage: string) => {
        toast.error(errorMessage);
    }, []);

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <Skeleton className="h-8 w-48" />
                    <Skeleton className="h-4 w-32 mt-2" />
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Feature Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-24 w-full" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !data?.featureById) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Feature</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading feature</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error?.message ?? 'Feature not found'}</p>
                        <Button variant="outline" className="mt-4" onClick={() => navigate('/features')}>
                            Back to Features
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    const feature = data.featureById;
    const tasks = feature.tasks as SimpleTask[] | undefined;

   return (
        <div className="space-y-6">
                <div className="flex items-center justify-between">
                     <div>
                         <div className="flex items-center gap-3">
                             <h2 className="text-2xl font-bold tracking-tight">{feature.title}</h2>
                             <Badge className={STATUS_COLORS[feature.status] || 'bg-gray-500'}>
                                 {feature.status}
                             </Badge>
                         </div>
                     </div>
                     <div className="flex gap-2">
                         <Button variant="outline" onClick={handleEditOpen}>
                             Edit
                         </Button>
                         <Button variant="outline" onClick={() => navigate('/features')}>
                             Back
                         </Button>
                     </div>
             </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="tasks">Tasks</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <MarkdownSection title="Description" content={feature.description} />
                    <MarkdownSection title="Acceptance Criteria" content={feature.acceptanceCriteria} />
                    <MarkdownSection title="Plan" content={feature.plan} />
                    <MarkdownSection title="Security Impact" content={feature.securityImpact} />
                    <MarkdownSection title="Performance Impact" content={feature.performanceImpact} />
                    <MarkdownSection title="Test Plan" content={feature.testPlan} />
                    <MarkdownSection title="Deployment Plan" content={feature.deploymentPlan} />
                    <MarkdownSection title="Open Questions" content={feature.openQuestions} />
                    <MarkdownSection title="Result" content={feature.result} />
                    <MarkdownSection title="Errors" content={feature.errors} />
                </TabsContent>

                <TabsContent value="tasks">
                    <TaskBoard
                        tasks={tasks as any[]}
                        featureId={feature.id}
                        onTasksChange={refetch}
                    />
                </TabsContent>
            </Tabs>

            <EditFeatureDialog
                open={isEditDialogOpen}
                onOpenChange={setIsEditDialogOpen}
                feature={feature ? {
                    id: feature.id,
                    title: feature.title,
                    description: feature.description,
                    acceptanceCriteria: feature.acceptanceCriteria,
                    plan: feature.plan,
                    securityImpact: feature.securityImpact,
                    performanceImpact: feature.performanceImpact,
                    testPlan: feature.testPlan,
                    deploymentPlan: feature.deploymentPlan,
                    openQuestions: feature.openQuestions,
                    updatedAt: feature.updatedAt,
                } : null}
                onSuccess={handleEditSuccess}
                onError={handleEditError}
            />
        </div>
    );
}
