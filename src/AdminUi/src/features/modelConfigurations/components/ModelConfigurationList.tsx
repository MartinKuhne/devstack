import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useModelConfigurations } from '@/features/modelConfigurations/hooks/useModelConfigurations';

interface ModelConfigurationListProps {
    projectId: string;
    onAddModel: () => void;
}

export function ModelConfigurationList({ projectId, onAddModel }: ModelConfigurationListProps) {
    const { modelConfigurations, loading, error } = useModelConfigurations(projectId);

    if (loading) {
        return (
            <div className="space-y-4">
                <div className="flex items-center justify-between">
                    <h3 className="text-lg font-semibold">Model Configurations</h3>
                    <Button onClick={onAddModel}>Add Model</Button>
                </div>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {[1, 2, 3].map((i) => (
                        <Card key={i}>
                            <CardHeader>
                                <div className="h-6 w-32 bg-muted animate-pulse rounded" />
                            </CardHeader>
                            <CardContent className="space-y-2">
                                <div className="h-4 w-full bg-muted animate-pulse rounded" />
                                <div className="h-4 w-2/3 bg-muted animate-pulse rounded" />
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <Card>
                <CardHeader>
                    <CardTitle className="text-destructive">Error loading model configurations</CardTitle>
                </CardHeader>
                <CardContent>
                    <p className="text-sm text-destructive">{error.message}</p>
                </CardContent>
            </Card>
        );
    }

    if (modelConfigurations.length === 0) {
        return (
            <Card>
                <CardHeader>
                    <div className="flex items-center justify-between">
                        <CardTitle>Model Configurations</CardTitle>
                        <Button onClick={onAddModel}>Add Model</Button>
                    </div>
                </CardHeader>
                <CardContent>
                    <p className="text-muted-foreground text-sm">
                        No model configurations yet. Click &quot;Add Model&quot; to get started.
                    </p>
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">Model Configurations</h3>
                <Button onClick={onAddModel}>Add Model</Button>
            </div>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {modelConfigurations.map((config) => (
                    <Card key={config.id}>
                        <CardHeader>
                            <CardTitle className="text-base">{config.modelAlias ?? config.model ?? ''}</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-3">
                            <div className="space-y-1">
                                <p className="text-xs text-muted-foreground">Model</p>
                                <p className="text-sm font-medium">{config.model}</p>
                            </div>
                            <div className="space-y-1">
                                <p className="text-xs text-muted-foreground">URL</p>
                                <p className="text-sm truncate" title={config.url ?? ''}>
                                    {config.url}
                                </p>
                            </div>
                            <div className="space-y-1">
                                <p className="text-xs text-muted-foreground">Max Complexity</p>
                                <Badge variant={getComplexityVariant(config.maxComplexity ?? 0)}>
                                    {config.maxComplexity}
                                </Badge>
                            </div>
                            <div className="text-xs text-muted-foreground">
                                Updated: {new Date(config.updatedAt).toLocaleDateString()}
                            </div>
                        </CardContent>
                    </Card>
                ))}
            </div>
        </div>
    );
}

function getComplexityVariant(maxComplexity: number): 'default' | 'secondary' | 'destructive' | 'outline' {
    if (maxComplexity <= 2) return 'secondary';
    if (maxComplexity <= 4) return 'default';
    if (maxComplexity <= 6) return 'outline';
    return 'destructive';
}
