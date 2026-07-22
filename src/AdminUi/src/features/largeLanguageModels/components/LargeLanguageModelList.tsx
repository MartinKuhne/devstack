import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Pencil, Trash2 } from 'lucide-react';
import { useLargeLanguageModels } from '@/features/largeLanguageModels/hooks/useLargeLanguageModels';
import { LargeLanguageModelDialog } from './LargeLanguageModelDialog';
import { useDeleteLargeLanguageModelMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';
import { LoadingState, ErrorState, EmptyState } from '@/components/layout';
import { ConfirmDialog } from '@/components/ConfirmDialog';

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
        cost: number;
        maxComplexity: number;
        maxConcurrency?: number;
    } | null>(null);
    const [deleteLargeLanguageModel, { loading: deleting }] = useDeleteLargeLanguageModelMutation();
    const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null);

    const handleEdit = (model: {
        id?: string | null | undefined;
        model?: string | null | undefined;
        modelAlias?: string | null | undefined;
        url?: string | null | undefined;
        cost?: number | null | undefined;
        maxComplexity?: number | null | undefined;
        maxConcurrency?: number | null | undefined;
    }) => {
        setEditingModel({
            id: model.id ?? '',
            model: model.model ?? '',
            url: model.url ?? '',
            cost: model.cost ?? 0,
            maxComplexity: model.maxComplexity ?? 0,
            maxConcurrency: model.maxConcurrency ?? 1,
        });
    };

    const handleDelete = async () => {
        if (!deleteTargetId) return;
        try {
            const result = await deleteLargeLanguageModel({
                variables: {
                    id: deleteTargetId,
                },
            });
            const deleted = result.data?.deleteLargeLanguageModel;
            if (!deleted) {
                toast.error('Failed to delete model');
            } else {
                toast.success('Model deleted successfully');
                onRefetch?.();
            }
        } catch {
            toast.error('Failed to delete model');
        }
        setDeleteTargetId(null);
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
                <LoadingState cards={3} rows={2} />
            </div>
        );
    }

    if (error) {
        return (
            <ErrorState
                message={error.message}
                title="Error loading large language models"
            />
        );
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">Large Language Models</h3>
                <Button onClick={onAddModel}>Add Model</Button>
            </div>
            {largeLanguageModels.length === 0 ? (
                <EmptyState
                    description="No large language models yet. Click &quot;Add Model&quot; to get started."
                    action={{ label: 'Add Model', onClick: onAddModel }}
                />
            ) : (
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {largeLanguageModels.map(
                        (
                            config: {
                                id: string | null | undefined;
                                model: string;
                                modelAlias?: string | null;
                                url: string;
                                cost: number;
                                maxComplexity: number;
                            } | null
                        ) =>
                            config ? (
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
                                                aria-label={`Edit model ${config.modelAlias ?? config.model ?? ''}`}
                                            >
                                                <Pencil className="h-4 w-4" />
                                            </Button>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 text-destructive hover:text-destructive"
                                                onClick={() => config.id && setDeleteTargetId(config.id)}
                                                disabled={deleting}
                                                aria-label={`Delete model ${config.modelAlias ?? config.model ?? ''}`}
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
                                            <p
                                                className="text-sm truncate"
                                                title={config.url ?? ''}
                                            >
                                                {config.url}
                                            </p>
                                        </div>
                                        <div className="space-y-1">
                                            <p className="text-xs text-muted-foreground">Cost</p>
                                            <p className="text-sm font-medium">{config.cost ?? 0}</p>
                                        </div>
                                        <div className="space-y-1">
                                            <p className="text-xs text-muted-foreground">
                                                Max Complexity
                                            </p>
                                            <Badge
                                                variant={getComplexityVariant(
                                                    config.maxComplexity ?? 0
                                                )}
                                            >
                                                {config.maxComplexity}
                                            </Badge>
                                        </div>
                                    </CardContent>
                                </Card>
                            ) : null
                    )}
                </div>
            )}
            <LargeLanguageModelDialog
                open={!!editingModel}
                onOpenChange={(open) => {
                    if (!open) handleCloseDialog();
                }}
                onSuccess={() => {
                    toast.success(editingModel ? 'Model updated successfully' : 'Model created successfully');
                    onRefetch?.();
                    handleCloseDialog();
                }}
                model={editingModel}
            />
            <ConfirmDialog
                open={!!deleteTargetId}
                onOpenChange={(open) => !open && setDeleteTargetId(null)}
                title="Delete Model"
                description="Are you sure you want to delete this model configuration? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />
        </div>
    );
}

function getComplexityVariant(
    maxComplexity: number
): 'default' | 'secondary' | 'outline' | 'warning' {
    if (maxComplexity <= 2) return 'secondary';
    if (maxComplexity <= 4) return 'default';
    if (maxComplexity <= 6) return 'outline';
    return 'warning';
}
