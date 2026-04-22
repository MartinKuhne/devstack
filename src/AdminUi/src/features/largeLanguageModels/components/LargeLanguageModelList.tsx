import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Pencil, Trash2 } from 'lucide-react';
import { useLargeLanguageModels } from '@/features/largeLanguageModels/hooks/useLargeLanguageModels';
import { LargeLanguageModelDialog } from './LargeLanguageModelDialog';
import { useDeleteLargeLanguageModelMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';

interface LargeLanguageModelListProps {
    onAddModel: () => void;
    onRefetch?: () => void;
}

export function LargeLanguageModelList({ onAddModel, onRefetch }: LargeLanguageModelListProps) {
    const { largeLanguageModels, loading, error } = useLargeLanguageModels();
    const [editingModel, setEditingModel] = useState<{
        id: string;
        model: string;
        modelAlias?: string;
        url: string;
        apiKey?: string;
        maxComplexity: number;
    } | null>(null);
    const [deleteLargeLanguageModel, { loading: deleting }] = useDeleteLargeLanguageModelMutation();

    const handleEdit = (model: {
        id?: string | null | undefined;
        model?: string | null | undefined;
        modelAlias?: string | null | undefined;
        url?: string | null | undefined;
        maxComplexity?: number | null | undefined;
    }) => {
        setEditingModel({
            id: model.id ?? '',
            model: model.model ?? '',
            url: model.url ?? '',
            maxComplexity: model.maxComplexity ?? 0,
        });
    };

    const handleDelete = async (modelId: string) => {
        if (!confirm('Are you sure you want to delete this model configuration?')) return;
        try {
            const result = await deleteLargeLanguageModel({
                variables: {
                    input: { id: modelId },
                },
            });
            if (result.data?.deleteLargeLanguageModel?.errors?.length) {
                const errorMessages = result.data.deleteLargeLanguageModel.errors.map((e: { field: string; message: string }) => e.message);
                toast.error(errorMessages.join(', '));
            } else {
                toast.success('Model deleted successfully');
                onRefetch?.();
            }
        } catch {
            toast.error('Failed to delete model');
        }
    };

    const handleCloseDialog = () => {
        setEditingModel(null);
    };

    if (loading) {
        return (
            <div className="space-y-4">
                <div className="flex items-center justify-between">
                    <h3 className="text-lg font-semibold">Large Language Models</h3>
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
                    <CardTitle className="text-destructive">
                        Error loading large language models
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <p className="text-sm text-destructive">{error.message}</p>
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">Large Language Models</h3>
                <Button onClick={onAddModel}>Add Model</Button>
            </div>
            {largeLanguageModels.length === 0 ? (
                <Card>
                    <CardContent className="pt-6">
                        <p className="text-muted-foreground text-sm">
                            No large language models yet. Click &quot;Add Model&quot; to get
                            started.
                        </p>
                    </CardContent>
                </Card>
            ) : (
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {largeLanguageModels.map((config) => (
                        <Card key={config.id}>
                            <CardHeader className="flex flex-row items-start justify-between space-y-0 pb-2">
                                <CardTitle className="text-base">
                                    {config.modelAlias ?? config.model ?? ''}
                                </CardTitle>
                                <div className="flex gap-1">
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-8 w-8"
                                        onClick={() => handleEdit(config)}
                                    >
                                        <Pencil className="h-4 w-4" />
                                    </Button>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-8 w-8 text-destructive hover:text-destructive"
                                        onClick={() => config.id && handleDelete(config.id)}
                                        disabled={deleting}
                                    >
                                        <Trash2 className="h-4 w-4" />
                                    </Button>
                                </div>
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
                                    <Badge
                                        variant={getComplexityVariant(config.maxComplexity ?? 0)}
                                    >
                                        {config.maxComplexity}
                                    </Badge>
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}
            <LargeLanguageModelDialog
                open={!!editingModel}
                onOpenChange={(open) => {
                    if (!open) handleCloseDialog();
                }}
                onSuccess={() => {
                    onRefetch?.();
                    handleCloseDialog();
                }}
                model={editingModel}
            />
        </div>
    );
}

function getComplexityVariant(
    maxComplexity: number
): 'default' | 'secondary' | 'destructive' | 'outline' {
    if (maxComplexity <= 2) return 'secondary';
    if (maxComplexity <= 4) return 'default';
    if (maxComplexity <= 6) return 'outline';
    return 'destructive';
}
