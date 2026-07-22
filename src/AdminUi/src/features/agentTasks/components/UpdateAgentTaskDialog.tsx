import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateAgentTaskMutation } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';
import { mapMutationError } from '@/lib/mapMutationError';
import { toast } from 'react-toastify';

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

const logger = createModuleLogger('UpdateAgentTaskDialog');

const agentTaskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(300, 'Title must be 300 characters or less'),
    description: z.string().optional(),
    complexityRating: z
        .number()
        .min(1, 'Complexity must be at least 1')
        .max(10, 'Complexity must be at most 10'),
    result: z.string().optional(),
});

type AgentTaskFormData = z.infer<typeof agentTaskSchema>;

interface AgentTaskData {
    id: string;
    title: string;
    description: string | null;
    complexityRating: number;
    result: string | null;
    status: string | null;
    deliverableId: string | null;
}

interface UpdateAgentTaskDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    agentTask: AgentTaskData | null;
    onSuccess?: () => void;
}

export function UpdateAgentTaskDialog({
    open,
    onOpenChange,
    agentTask,
    onSuccess,
}: UpdateAgentTaskDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateAgentTask, { loading }] = useUpdateAgentTaskMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<AgentTaskFormData>({
        resolver: zodResolver(agentTaskSchema),
    });

    useEffect(() => {
        if (agentTask && open) {
            reset({
                title: agentTask.title,
                description: agentTask.description ?? '',
                complexityRating: agentTask.complexityRating,
                result: agentTask.result ?? '',
            });
        }
    }, [agentTask, open, reset]);

    const onSubmit = async (data: AgentTaskFormData) => {
        if (!agentTask) return;

        setServerError(null);
        logger.info('Updating agent task', { id: agentTask.id, title: data.title });

        try {
            const result = await updateAgentTask({
                variables: {
                    input: {
                        id: agentTask.id,
                        title: data.title,
                        description: data.description ?? null,
                        result: data.result ?? null,
                        complexityRating: Number(data.complexityRating),
                    },
                },
            });

            const payload = result.data?.updateAgentTask;
            if (payload?.errors) {
                logger.warn('Failed to update agent task', {
                    id: agentTask.id,
                    errors: payload.errors,
                });
                if (payload.errors.includes('NOT_FOUND')) {
                    logger.warn('Agent task not found during update, closing dialog', {
                        id: agentTask.id,
                    });
                    onSuccess?.();
                    onOpenChange(false);
                    return;
                } else {
                    const friendlyError = mapMutationError(
                        new Error(payload.errors),
                        'agent task'
                    );
                    setServerError(friendlyError);
                }
                return;
            }

            logger.info('Agent task updated successfully', { id: agentTask.id, title: data.title });
            toast.success('Agent task updated successfully');
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            const errorInfo = mapMutationError(err, 'agent task');
            logger.error('Failed to update agent task', {
                id: agentTask.id,
                title: data.title,
                error: errorInfo,
            });
            setServerError(errorInfo);
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        setServerError(null);
        if (!isOpen) {
            reset();
        }
        onOpenChange(isOpen);
    };

    if (!agentTask) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Agent Task</DialogTitle>
                    <DialogDescription>Update agent task details.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Task title"
                                aria-invalid={!!errors.title}
                                aria-describedby={errors.title ? 'title-error' : undefined}
                            />
                            {errors.title && (
                                <p id="title-error" className="text-sm text-destructive" role="alert">
                                    {errors.title.message}
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
                                aria-invalid={!!errors.complexityRating}
                                aria-describedby={errors.complexityRating ? 'complexity-error' : undefined}
                            />
                            {errors.complexityRating && (
                                <p id="complexity-error" className="text-sm text-destructive" role="alert">
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

                        {serverError && <p className="text-sm text-destructive" role="alert">{serverError}</p>}
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => handleOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={loading}>
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
