import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useCreateAgentTaskMutation } from '@/generated/graphql';
import { createModuleLogger, formatGraphQLError } from '@/lib/logging';

import { Button } from '@/components/ui/button';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const logger = createModuleLogger('CreateAgentTaskDialog');

const agentTaskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(300, 'Title must be 300 characters or less'),
    deliverableId: z.string().min(1, 'Deliverable ID is required'),
    description: z.string().optional(),
    complexityRating: z
        .number()
        .min(1, 'Complexity must be at least 1')
        .max(10, 'Complexity must be at most 10'),
    result: z.string().optional(),
    dependsOnAgentTaskId: z.string().optional(),
});

type AgentTaskFormData = z.infer<typeof agentTaskSchema>;

interface CreateAgentTaskDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    deliverableId: string;
    onSuccess?: (agentTaskId: string) => void;
}

export function CreateAgentTaskDialog({
    open,
    onOpenChange,
    deliverableId,
    onSuccess,
}: CreateAgentTaskDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createAgentTask, { loading }] = useCreateAgentTaskMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isValid },
    } = useForm<AgentTaskFormData>({
        resolver: zodResolver(agentTaskSchema),
        defaultValues: {
            deliverableId,
            complexityRating: 5,
        },
    });

    const onSubmit = async (data: AgentTaskFormData) => {
        setServerError(null);
        logger.info('Creating agent task', {
            deliverableId: data.deliverableId,
            title: data.title,
        });

        try {
            const result = await createAgentTask({
                variables: {
                    input: {
                        deliverableId: data.deliverableId,
                        title: data.title,
                        description: data.description ?? '',
                        complexityRating: Number(data.complexityRating),
                        result: data.result ?? null,
                    },
                },
            });

            const payload = result.data?.createAgentTask;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                logger.warn('Failed to create agent task', {
                    deliverableId: data.deliverableId,
                    title: data.title,
                    errors: payload.errors,
                });
                setServerError(errorMessage);
                return;
            }

            logger.info('Agent task created successfully', {
                id: payload?.agentTask?.id,
                title: data.title,
            });
            reset();
            onSuccess?.(payload?.agentTask?.id ?? '');
            onOpenChange(false);
        } catch (err) {
            const errorInfo = formatGraphQLError(err);
            logger.error('Failed to create agent task', {
                deliverableId: data.deliverableId,
                title: data.title,
                error: errorInfo.message,
                details: errorInfo.details,
            });
            setServerError(err instanceof Error ? err.message : 'Failed to create agent task');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Agent Task</DialogTitle>
                    <DialogDescription>Add a new agent task for execution.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input id="title" {...register('title')} placeholder="Task title" />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="deliverableId">Deliverable ID *</Label>
                            <Input
                                id="deliverableId"
                                {...register('deliverableId')}
                                placeholder="UUID of the deliverable"
                                disabled
                            />
                            {errors.deliverableId && (
                                <p className="text-sm text-destructive">
                                    {errors.deliverableId.message}
                                </p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="complexityRating">Complexity Rating (1-10) *</Label>
                            <Input
                                id="complexityRating"
                                type="number"
                                min={1}
                                max={10}
                                {...register('complexityRating')}
                            />
                            {errors.complexityRating && (
                                <p className="text-sm text-destructive">
                                    {errors.complexityRating.message}
                                </p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                {...register('description')}
                                placeholder="Task description"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="result">Result</Label>
                            <Textarea
                                id="result"
                                {...register('result')}
                                placeholder="Task result"
                                rows={3}
                            />
                        </div>

                        {serverError && <p className="text-sm text-destructive">{serverError}</p>}
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => handleOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={!isValid || loading}>
                            {loading ? 'Creating...' : 'Create Agent Task'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
