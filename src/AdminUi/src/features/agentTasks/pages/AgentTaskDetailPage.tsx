import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Separator } from '@/components/ui/separator';
import { useAgentTask } from '../hooks/useAgentTask';
import { UpdateAgentTaskDialog } from '../components/UpdateAgentTaskDialog';
import { useState } from 'react';

const STATUS_COLORS: Record<string, string> = {
    READY: 'bg-blue-500',
    IN_PROGRESS: 'bg-yellow-500',
    NEEDS_REVIEW: 'bg-purple-500',
    DONE: 'bg-green-500',
    FAILED: 'bg-red-500',
    REJECTED: 'bg-gray-500',
};

export function AgentTaskDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { agentTask, loading, error, refetch } = useAgentTask(id ?? '');
    const [updateDialogOpen, setUpdateDialogOpen] = useState(false);

    if (loading) {
        return (
            <div className="space-y-6">
                <div>
                    <div className="h-8 w-64 bg-muted rounded" />
                    <div className="h-4 w-32 mt-2 bg-muted rounded" />
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle>Agent Task Details</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="h-20 bg-muted rounded" />
                        <div className="h-20 bg-muted rounded" />
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error || !agentTask) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Agent Task</h2>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading agent task</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error?.message ?? 'Agent task not found'}</p>
                        <Button variant="outline" className="mt-4" onClick={() => navigate('/agent-tasks')}>
                            Back to Agent Tasks
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
                    <h2 className="text-2xl font-bold tracking-tight">{agentTask.title}</h2>
                    <div className="flex items-center gap-2 mt-2">
                        <Badge className={STATUS_COLORS[agentTask.status ?? ''] || 'bg-gray-500'}>
                            {agentTask.status}
                        </Badge>
                        <span className="text-sm text-muted-foreground">{agentTask.deliverable}</span>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => navigate('/agent-tasks')}>
                        Back to List
                    </Button>
                    <Button onClick={() => setUpdateDialogOpen(true)}>
                        Edit
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="telemetry">Telemetry</TabsTrigger>
                    <TabsTrigger value="dependencies">Dependencies</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Description</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-sm whitespace-pre-wrap">
                                {agentTask.result ?? 'No result provided.'}
                            </p>
                        </CardContent>
                    </Card>

                    {agentTask.acceptanceCriteria && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Acceptance Criteria</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{agentTask.acceptanceCriteria}</p>
                            </CardContent>
                        </Card>
                    )}

                    {agentTask.risks && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Risks</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{agentTask.risks}</p>
                            </CardContent>
                        </Card>
                    )}

                    {agentTask.requiredFollowUps && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Required Follow-ups</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{agentTask.requiredFollowUps}</p>
                            </CardContent>
                        </Card>
                    )}

                    <div className="text-sm text-muted-foreground">
                        <p>Complexity Rating: {agentTask.complexityRating ?? '-'}</p>
                        <p>Created: {agentTask.createdAt ? new Date(agentTask.createdAt).toLocaleString() : '-'}</p>
                        <p>Updated: {agentTask.updatedAt ? new Date(agentTask.updatedAt).toLocaleString() : '-'}</p>
                    </div>
                </TabsContent>

                <TabsContent value="telemetry" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Execution Telemetry</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <p className="text-sm text-muted-foreground">Model</p>
                                    <p className="font-medium">{agentTask.model || '-'}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Commit Hash</p>
                                    <p className="font-medium font-mono text-xs">{agentTask.commitHash || '-'}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Prompt Tokens</p>
                                    <p className="font-medium">{agentTask.promptTokens ?? 0}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Completion Tokens</p>
                                    <p className="font-medium">{agentTask.completionTokens ?? 0}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-muted-foreground">Execution Duration</p>
                                    <p className="font-medium">{agentTask.executionDurationInSeconds ?? 0} seconds</p>
                                </div>
                            </div>
                            {agentTask.errors && (
                                <>
                                    <Separator />
                                    <div>
                                        <p className="text-sm text-muted-foreground mb-2">Errors</p>
                                        <p className="text-sm text-destructive whitespace-pre-wrap">{agentTask.errors}</p>
                                    </div>
                                </>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="dependencies" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Dependencies</CardTitle>
                        </CardHeader>
                        <CardContent>
                            {agentTask.dependsOnAgentTask && agentTask.dependsOnAgentTask.length > 0 ? (
                                <div className="space-y-2">
                                    {agentTask.dependsOnAgentTask.map((dep) => (
                                        <div key={dep?.id ?? ''} className="flex items-center gap-2 p-2 bg-muted rounded">
                                            <Badge variant="outline">{dep?.id}</Badge>
                                            <span className="text-sm">{dep?.title}</span>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <p className="text-sm text-muted-foreground">No dependencies.</p>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>

            <UpdateAgentTaskDialog
                open={updateDialogOpen}
                onOpenChange={setUpdateDialogOpen}
                agentTask={{
                    id: agentTask.id ?? '',
                    title: agentTask.title ?? '',
                    deliverable: agentTask.deliverable,
                    acceptanceCriteria: agentTask.acceptanceCriteria,
                    risks: agentTask.risks,
                    requiredFollowUps: agentTask.requiredFollowUps,
                    complexityRating: agentTask.complexityRating ?? 0,
                    result: agentTask.result,
                    status: agentTask.status,
                    itemId: agentTask.itemId,
                }}
                onSuccess={() => refetch()}
            />
        </div>
    );
}
