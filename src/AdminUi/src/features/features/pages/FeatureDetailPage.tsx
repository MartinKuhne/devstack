import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useFeature } from '@/features/features/hooks/useFeature';

const STATUS_COLORS: Record<string, string> = {
    Planned: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Review: 'bg-purple-500',
    Done: 'bg-green-500',
    Failed: 'bg-red-500',
};

interface Task {
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
    const { data, loading, error } = useFeature(id ?? '');

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
    const tasks = feature.tasks as Task[] | undefined;

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
                    <Button variant="outline" onClick={() => navigate('/features')}>
                        Back
                    </Button>
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
                    <Card>
                        <CardHeader>
                            <CardTitle>Tasks</CardTitle>
                        </CardHeader>
                        <CardContent>
                            {tasks && tasks.length > 0 ? (
                                <div className="space-y-2">
                                    {tasks.map((task) => (
                                        <div
                                            key={task.id}
                                            className="flex items-center justify-between p-2 border rounded"
                                        >
                                            <span>{task.title}</span>
                                            <Badge variant="outline">{task.status}</Badge>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <p className="text-muted-foreground text-sm">No tasks yet.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>
        </div>
    );
}
