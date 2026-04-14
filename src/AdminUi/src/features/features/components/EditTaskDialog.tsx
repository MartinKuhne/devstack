import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { gql } from '@apollo/client/core';
import { useMutation } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { UpdateTaskMutation, UpdateTaskMutationVariables } from '@/generated/graphql';
import { toast } from 'react-toastify';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const UPDATE_TASK = gql`
    mutation UpdateTask($id: ID!, $input: UpdateTaskInput!) {
        updateTask(id: $id, input: $input) {
            id
            title
            deliverable
            acceptanceCriteria
            risks
            requiredFollowUps
            complexity
            status
            updatedAt
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
    status: z.enum(['Todo', 'InProgress', 'Review', 'Done']),
});

type TaskFormData = z.infer<typeof taskSchema>;

interface TaskData {
    id: string;
    title: string;
    deliverable: string | null;
    acceptanceCriteria: string | null;
    risks: string | null;
    requiredFollowUps: string | null;
    complexity: string;
    status: string;
    updatedAt: string;
    version: number;
}

interface EditTaskDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    task: TaskData | null;
    onSuccess?: () => void;
}

export function EditTaskDialog({ open, onOpenChange, task, onSuccess }: EditTaskDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateTask, { loading }] = useMutation<UpdateTaskMutation, UpdateTaskMutationVariables>(UPDATE_TASK, {
        client: getApolloClient(),
    });

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        formState: { errors },
    } = useForm<TaskFormData>({
        resolver: zodResolver(taskSchema),
    });

    useEffect(() => {
        if (task && open) {
            reset({
                title: task.title,
                deliverable: task.deliverable ?? '',
                acceptanceCriteria: task.acceptanceCriteria ?? '',
                risks: task.risks ?? '',
                requiredFollowUps: task.requiredFollowUps ?? '',
                complexity: task.complexity as 'Simple' | 'Moderate' | 'Complex' | 'Major',
                status: task.status as 'Todo' | 'InProgress' | 'Review' | 'Done',
            });
            setServerError(null);
        }
    }, [task, open, reset]);

    const onSubmit = async (data: TaskFormData) => {
        if (!task) return;
        
        setServerError(null);
        
        try {
            await updateTask({
                variables: {
                    id: task.id,
                    input: {
                        title: data.title,
                        deliverable: data.deliverable ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        risks: data.risks ?? null,
                        requiredFollowUps: data.requiredFollowUps ?? null,
                        complexity: data.complexity,
                        status: data.status,
                        version: task.version,
                    },
                },
            });

            toast.success('Task updated successfully');
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to update task';
            
            if (errorMessage.includes('NOT_FOUND')) {
                toast.error('Task not found. It may have been deleted.');
                onOpenChange(false);
            } else if (errorMessage.includes('CONCURRENT') || errorMessage.includes('version')) {
                toast.error('The task was modified by another user. Please refresh and try again.');
                onOpenChange(false);
            } else {
                setServerError(errorMessage);
            }
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    if (!task) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Task</DialogTitle>
                    <DialogDescription>
                        Update task details. Changes will be saved immediately.
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
                            <Label htmlFor="complexity">Complexity *</Label>
                            <Select 
                                defaultValue={task.complexity} 
                                onValueChange={(value) => setValue('complexity', value as any)}
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select complexity" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Simple">Simple</SelectItem>
                                    <SelectItem value="Moderate">Moderate</SelectItem>
                                    <SelectItem value="Complex">Complex</SelectItem>
                                    <SelectItem value="Major">Major</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.complexity && (
                                <p className="text-sm text-destructive">{errors.complexity.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="status">Status *</Label>
                            <Select 
                                defaultValue={task.status} 
                                onValueChange={(value) => setValue('status', value as any)}
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select status" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Todo">Todo</SelectItem>
                                    <SelectItem value="InProgress">InProgress</SelectItem>
                                    <SelectItem value="Review">Review</SelectItem>
                                    <SelectItem value="Done">Done</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.status && (
                                <p className="text-sm text-destructive">{errors.status.message}</p>
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
