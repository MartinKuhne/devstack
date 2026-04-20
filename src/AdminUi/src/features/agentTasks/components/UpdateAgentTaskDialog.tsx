import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateAgentTaskMutation } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const agentTaskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(300, 'Title must be 300 characters or less'),
    deliverable: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    risks: z.string().optional(),
    requiredFollowUps: z.string().optional(),
    complexityRating: z.number().min(1, 'Complexity must be at least 1').max(10, 'Complexity must be at most 10'),
    result: z.string().optional(),
});

type AgentTaskFormData = z.infer<typeof agentTaskSchema>;

interface AgentTaskData {
    id: string;
    title: string;
    deliverable: string | null;
    acceptanceCriteria: string | null;
    risks: string | null;
    requiredFollowUps: string | null;
    complexityRating: number;
    result: string | null;
    status: string | null;
    itemId: string | null;
}

interface UpdateAgentTaskDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    agentTask: AgentTaskData | null;
    onSuccess?: () => void;
}

export function UpdateAgentTaskDialog({ open, onOpenChange, agentTask, onSuccess }: UpdateAgentTaskDialogProps) {
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
                deliverable: agentTask.deliverable ?? '',
                acceptanceCriteria: agentTask.acceptanceCriteria ?? '',
                risks: agentTask.risks ?? '',
                requiredFollowUps: agentTask.requiredFollowUps ?? '',
                complexityRating: agentTask.complexityRating,
                result: agentTask.result ?? '',
            });
            setServerError(null);
        }
    }, [agentTask, open, reset]);

    const onSubmit = async (data: AgentTaskFormData) => {
        if (!agentTask) return;

        setServerError(null);

        try {
            const result = await updateAgentTask({
                variables: {
                    input: {
                        id: agentTask.id,
                        title: data.title,
                        deliverable: data.deliverable ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        risks: data.risks ?? null,
                        requiredFollowUps: data.requiredFollowUps ?? null,
                        result: data.result ?? null,
                        complexityRating: Number(data.complexityRating),
                        commitHash: null,
                        completionTokens: null,
                        errors: null,
                        executionDurationInSeconds: null,
                        agent: null,
                        promptTokens: null,
                    },
                },
            });

            const payload = result.data?.updateAgentTask;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                if (errorMessage.includes('NOT_FOUND')) {
                    setServerError('Agent task not found. It may have been deleted.');
                } else if (errorMessage.includes('CONCURRENCY_CONFLICT')) {
                    setServerError('The agent task was modified by another user. Please refresh and try again.');
                } else {
                    setServerError(errorMessage);
                }
                return;
            }

            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to update agent task');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    if (!agentTask) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Agent Task</DialogTitle>
                    <DialogDescription>
                        Update agent task details.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Task title"
                            />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
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
                                <p className="text-sm text-destructive">{errors.complexityRating.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="deliverable">Deliverable</Label>
                            <Textarea
                                id="deliverable"
                                {...register('deliverable')}
                                placeholder="What will be delivered"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="Criteria for completing this task"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="risks">Risks</Label>
                            <Textarea
                                id="risks"
                                {...register('risks')}
                                placeholder="Identified risks"
                                rows={2}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="requiredFollowUps">Required Follow-ups</Label>
                            <Textarea
                                id="requiredFollowUps"
                                {...register('requiredFollowUps')}
                                placeholder="Required follow-up actions"
                                rows={2}
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

                        {serverError && (
                            <p className="text-sm text-destructive">{serverError}</p>
                        )}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => handleOpenChange(false)}>
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
