import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { gql } from '@apollo/client/core';
import { useMutation } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { CreateTaskMutation, CreateTaskMutationVariables } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const CREATE_TASK = gql`
    mutation CreateTask($input: CreateTaskInput!) {
        createTask(input: $input) {
            id
            title
            deliverable
            acceptanceCriteria
            risks
            requiredFollowUps
            complexity
            status
        }
    }
`;

const taskSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    deliverable: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    risks: z.string().optional(),
    requiredFollowUps: z.string().optional(),
    complexity: z.enum(['Simple', 'Moderate', 'Complex', 'Major']),
    featureId: z.string().min(1, 'Feature is required'),
});

type TaskFormData = z.infer<typeof taskSchema>;

interface CreateTaskDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    featureId: string;
    onSuccess?: (taskId: string) => void;
}

export function CreateTaskDialog({ open, onOpenChange, featureId, onSuccess }: CreateTaskDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createTask, { loading }] = useMutation<CreateTaskMutation, CreateTaskMutationVariables>(CREATE_TASK, {
        client: getApolloClient(),
    });

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<TaskFormData>({
        resolver: zodResolver(taskSchema),
        defaultValues: {
            complexity: 'Moderate' as const,
        },
    });

    const onSubmit = async (data: TaskFormData) => {
        setServerError(null);
        
        try {
            const result = await createTask({
                variables: {
                    input: {
                        featureId: data.featureId,
                        title: data.title,
                        deliverable: data.deliverable ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        risks: data.risks ?? null,
                        requiredFollowUps: data.requiredFollowUps ?? null,
                        complexity: data.complexity,
                    },
                },
            });

            reset();
            onSuccess?.(result.data?.createTask.id ?? '');
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to create task');
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
            <DialogContent className="sm:max-w-[500px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Task</DialogTitle>
                    <DialogDescription>
                        Add a new task to this feature.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <input type="hidden" {...register('featureId')} value={featureId} />

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
                            <Label htmlFor="deliverable">Deliverable</Label>
                            <Textarea
                                id="deliverable"
                                {...register('deliverable')}
                                placeholder="What will be delivered?"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="Criteria for task completion"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="risks">Risks</Label>
                            <Textarea
                                id="risks"
                                {...register('risks')}
                                placeholder="Potential risks or challenges"
                                rows={2}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="requiredFollowUps">Required Follow-ups</Label>
                            <Textarea
                                id="requiredFollowUps"
                                {...register('requiredFollowUps')}
                                placeholder="Follow-up actions needed"
                                rows={2}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="complexity">Complexity *</Label>
                            <Select defaultValue="Moderate" onValueChange={(value) => {
                                register('complexity').onChange({ target: { value } });
                            }}>
                                <SelectTrigger>
                                    <SelectValue placeholder="Select complexity" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Simple">Simple (1-3)</SelectItem>
                                    <SelectItem value="Moderate">Moderate (4-6)</SelectItem>
                                    <SelectItem value="Complex">Complex (7-8)</SelectItem>
                                    <SelectItem value="Major">Major (9-10)</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.complexity && (
                                <p className="text-sm text-destructive">{errors.complexity.message}</p>
                            )}
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
                            {loading ? 'Creating...' : 'Create Task'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}


