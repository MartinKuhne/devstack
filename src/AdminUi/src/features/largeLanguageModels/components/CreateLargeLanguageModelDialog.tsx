import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
    DialogDescription,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { useCreateLargeLanguageModelMutation } from '@/generated/graphql';
import { createModuleLogger, formatGraphQLError } from '@/lib/logging';

const logger = createModuleLogger('CreateLargeLanguageModelDialog');

const llmSchema = z.object({
    url: z.string().min(1, 'URL is required').url('Invalid URL format'),
    model: z.string().min(1, 'Model name is required'),
    modelAlias: z.string().optional(),
    cost: z.number().min(0, 'Cost must be at least 0').max(100, 'Cost must be at most 100'),
    apiKey: z.string().min(1, 'API key is required'),
    maxComplexity: z.number().min(1, 'Must be at least 1').max(10, 'Must be at most 10'),
    maxConcurrency: z.number().min(1, 'Must be at least 1').max(100, 'Must be at most 100'),
});

type LlmFormData = z.infer<typeof llmSchema>;

interface CreateLargeLanguageModelDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess: () => void;
}

export function CreateLargeLanguageModelDialog({
    open,
    onOpenChange,
    onSuccess,
}: CreateLargeLanguageModelDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [showApiKey, setShowApiKey] = useState(false);

    const [createLargeLanguageModel, { loading }] =
        useCreateLargeLanguageModelMutation();

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        formState: { errors, isValid },
    } = useForm<LlmFormData>({
        resolver: zodResolver(llmSchema),
        defaultValues: {
            url: '',
            model: '',
            modelAlias: '',
            cost: 0,
            apiKey: '',
            maxComplexity: 3,
            maxConcurrency: 1,
        },
    });

    const handleOpenChange = (newOpen: boolean) => {
        setServerError(null);
        if (!newOpen) {
            reset();
        }
        onOpenChange(newOpen);
    };

    const onSubmit = async (data: LlmFormData) => {
        setServerError(null);
        try {
            logger.info('Creating LLM model', { model: data.model });
            const result = await createLargeLanguageModel({
                variables: {
                    input: {
                        model: data.model,
                        modelAlias: data.modelAlias || null,
                        url: data.url,
                        apiKey: data.apiKey,
                        cost: data.cost,
                        maxComplexity: data.maxComplexity,
                        maxConcurrency: data.maxConcurrency,
                    },
                },
            });
            const createdModel = result.data?.createLargeLanguageModel;
            if (!createdModel) {
                logger.warn('Failed to create LLM model', { model: data.model });
                setServerError('Failed to create LLM model');
                return;
            }
            logger.info('LLM model created successfully', { model: data.model });
            reset();
            onOpenChange(false);
            onSuccess();
        } catch (err) {
            const errorInfo = formatGraphQLError(err);
            logger.error('Failed to create LLM model', {
                model: data.model,
                error: errorInfo.message,
                details: errorInfo.details,
            });
            setServerError(err instanceof Error ? err.message : 'An unexpected error occurred');
        }
    };

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Add Large Language Model</DialogTitle>
                    <DialogDescription>
                        Configure a new model endpoint for this project. API keys are encrypted server-side.
                    </DialogDescription>
                </DialogHeader>

                {serverError && (
                    <div className="text-sm text-destructive bg-destructive/10 p-3 rounded-md">
                        {serverError}
                    </div>
                )}

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="create-url">Endpoint URL</Label>
                            <Input
                                id="create-url"
                                {...register('url')}
                                placeholder="https://api.example.com/v1"
                            />
                            {errors.url && <p className="text-sm text-destructive">{errors.url.message}</p>}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-model">Model Name</Label>
                            <Input
                                id="create-model"
                                {...register('model')}
                                placeholder="gpt-4o-mini"
                            />
                            {errors.model && <p className="text-sm text-destructive">{errors.model.message}</p>}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-modelAlias">Alias (optional)</Label>
                            <Input
                                id="create-modelAlias"
                                {...register('modelAlias')}
                                placeholder="Default"
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-cost">Cost (0-100)</Label>
                            <Input
                                id="create-cost"
                                type="number"
                                min={0}
                                max={100}
                                {...register('cost', { valueAsNumber: true })}
                                placeholder="0"
                            />
                            {errors.cost && <p className="text-sm text-destructive">{errors.cost.message}</p>}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-apiKey">API Key</Label>
                            <div className="flex gap-2">
                                <Input
                                    id="create-apiKey"
                                    type={showApiKey ? 'text' : 'password'}
                                    {...register('apiKey')}
                                    placeholder="sk-..."
                                />
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => setShowApiKey(!showApiKey)}
                                >
                                    {showApiKey ? 'Hide' : 'Show'}
                                </Button>
                            </div>
                            {errors.apiKey && <p className="text-sm text-destructive">{errors.apiKey.message}</p>}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-maxComplexity">Max Complexity (1-10)</Label>
                            <Select
                                value="3"
                                onValueChange={(value) => setValue('maxComplexity', Number(value))}
                            >
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
                            {errors.maxComplexity && <p className="text-sm text-destructive">{errors.maxComplexity.message}</p>}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="create-maxConcurrency">Max Concurrency (1-100)</Label>
                            <Input
                                id="create-maxConcurrency"
                                type="number"
                                min={1}
                                max={100}
                                {...register('maxConcurrency', { valueAsNumber: true })}
                                placeholder="1"
                            />
                            {errors.maxConcurrency && <p className="text-sm text-destructive">{errors.maxConcurrency.message}</p>}
                        </div>
                    </div>
                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => handleOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={loading || !isValid}>
                            {loading ? 'Creating...' : 'Add Model'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
