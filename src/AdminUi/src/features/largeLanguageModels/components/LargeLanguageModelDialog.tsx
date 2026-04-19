import { useState, useEffect } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useCreateModelConfigurationMutation, useUpdateModelConfigurationMutation, useDeleteModelConfigurationMutation } from '@/generated/graphql';

interface LargeLanguageModelDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess: () => void;
    model?: {
        id: string;
        model: string;
        modelAlias?: string;
        url: string;
        apiKey?: string;
        maxComplexity: number;
    } | null;
}

export function LargeLanguageModelDialog({
    open,
    onOpenChange,
    onSuccess,
    model,
}: LargeLanguageModelDialogProps) {
    const isEditMode = !!model;
    const [modelValue, setModelValue] = useState('');
    const [modelAlias, setModelAlias] = useState('');
    const [url, setUrl] = useState('');
    const [apiKey, setApiKey] = useState('');
    const [maxComplexity, setMaxComplexity] = useState('3');
    const [showApiKey, setShowApiKey] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [createModelConfiguration, { loading: createLoading }] = useCreateModelConfigurationMutation();
    const [updateModelConfiguration, { loading: updateLoading }] = useUpdateModelConfigurationMutation();
    const [deleteModelConfiguration, { loading: deleteLoading }] = useDeleteModelConfigurationMutation();

    const resetForm = () => {
        setModelValue('');
        setModelAlias('');
        setUrl('');
        setApiKey('');
        setMaxComplexity('3');
        setShowApiKey(false);
        setError(null);
    };

    useEffect(() => {
        if (open && model) {
            setModelValue(model.model);
            setModelAlias(model.modelAlias ?? '');
            setUrl(model.url);
            setApiKey(model.apiKey ?? '');
            setMaxComplexity(model.maxComplexity.toString());
            setShowApiKey(false);
            setError(null);
        } else if (open && !model) {
            resetForm();
        }
    }, [open, model]);

    const handleOpenChange = (newOpen: boolean) => {
        if (!newOpen) {
            resetForm();
        }
        onOpenChange(newOpen);
    };

    const validateForm = () => {
        if (!url.trim()) {
            return { valid: false, error: 'URL is required' };
        }
        try {
            new URL(url);
        } catch {
            return { valid: false, error: 'Invalid URL format' };
        }
        if (!modelValue.trim()) {
            return { valid: false, error: 'Model name is required' };
        }
        if (!apiKey.trim()) {
            return { valid: false, error: 'API key is required' };
        }
        const complexity = parseInt(maxComplexity, 10);
        if (isNaN(complexity) || complexity < 1 || complexity > 10) {
            return { valid: false, error: 'Max complexity must be between 1 and 10' };
        }
        return { valid: true };
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        const validationResult = validateForm();
        if (!validationResult.valid) {
            setError(validationResult.error!);
            return;
        }

        try {
            if (isEditMode && model) {
                const result = await updateModelConfiguration({
                    variables: {
                        input: {
                            id: model.id,
                            model: modelValue,
                            modelAlias: modelAlias || null,
                            url,
                            apiKey,
                            maxComplexity: parseInt(maxComplexity, 10),
                        },
                    },
                });
                const payload = result.data?.updateModelConfiguration;
                if (payload?.errors?.length) {
                    setError(payload.errors.join(', '));
                    return;
                }
            } else {
                const result = await createModelConfiguration({
                    variables: {
                        input: {
                            model: modelValue,
                            modelAlias: modelAlias || null,
                            url,
                            apiKey,
                            maxComplexity: parseInt(maxComplexity, 10),
                        },
                    },
                });
                const payload = result.data?.createModelConfiguration;
                if (payload?.errors?.length) {
                    setError(payload.errors.join(', '));
                    return;
                }
            }

            resetForm();
            onOpenChange(false);
            onSuccess();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'An unexpected error occurred');
        }
    };

    const handleDelete = async () => {
        if (!model) return;
        setError(null);

        try {
            const result = await deleteModelConfiguration({
                variables: {
                    input: { id: model.id },
                },
            });
            const payload = result.data?.deleteModelConfiguration;
            if (payload?.errors?.length) {
                setError(payload.errors.join(', '));
                return;
            }
            resetForm();
            onOpenChange(false);
            onSuccess();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to delete model');
        }
    };

    const isLoading = createLoading || updateLoading || deleteLoading;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>{isEditMode ? 'Edit Large Language Model' : 'Add Large Language Model'}</DialogTitle>
                    <DialogDescription>
                        {isEditMode
                            ? 'Update the model endpoint configuration. API keys are encrypted server-side.'
                            : 'Configure a new model endpoint for this project. API keys are encrypted server-side.'}
                    </DialogDescription>
                </DialogHeader>

                {error && (
                    <div className="text-sm text-destructive bg-destructive/10 p-3 rounded-md">{error}</div>
                )}

                <form onSubmit={handleSubmit}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="url">Endpoint URL</Label>
                            <Input
                                id="url"
                                value={url}
                                onChange={(e) => setUrl(e.target.value)}
                                placeholder="https://api.example.com/v1"
                                required
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="model">Model Name</Label>
                            <Input
                                id="model"
                                value={modelValue}
                                onChange={(e) => setModelValue(e.target.value)}
                                placeholder="gpt-4o-mini"
                                required
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="alias">Alias (optional)</Label>
                            <Input
                                id="alias"
                                value={modelAlias}
                                onChange={(e) => setModelAlias(e.target.value)}
                                placeholder="Default"
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="apiKey">API Key</Label>
                            <div className="flex gap-2">
                                <Input
                                    id="apiKey"
                                    type={showApiKey ? 'text' : 'password'}
                                    value={apiKey}
                                    onChange={(e) => setApiKey(e.target.value)}
                                    placeholder="sk-..."
                                    required
                                />
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => setShowApiKey(!showApiKey)}
                                >
                                    {showApiKey ? 'Hide' : 'Show'}
                                </Button>
                            </div>
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="complexity">Max Complexity (1-10)</Label>
                            <Select value={maxComplexity} onValueChange={setMaxComplexity}>
                                <SelectTrigger>
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((num) => (
                                        <SelectItem key={num} value={num.toString()}>
                                            {num}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                    <DialogFooter>
                        {isEditMode && (
                            <Button
                                type="button"
                                variant="destructive"
                                onClick={handleDelete}
                                disabled={isLoading}
                                className="mr-auto"
                            >
                                {deleteLoading ? 'Deleting...' : 'Delete'}
                            </Button>
                        )}
                        <Button type="button" variant="outline" onClick={() => handleOpenChange(false)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={isLoading}>
                            {isLoading
                                ? (isEditMode ? 'Updating...' : 'Creating...')
                                : (isEditMode ? 'Save' : 'Add Model')}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
