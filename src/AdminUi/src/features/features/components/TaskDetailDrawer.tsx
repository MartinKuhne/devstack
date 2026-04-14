import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Drawer, DrawerContent, DrawerHeader, DrawerTitle, DrawerDescription, DrawerFooter } from '@/components/ui/drawer';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { useState, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { gql } from '@apollo/client/core';
import { useMutation } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { Task, UpdateTaskMutation, UpdateTaskMutationVariables, TaskComplexity, TaskStatus } from '@/generated/graphql';
import { toast } from 'react-toastify';

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
        }
    }
`;

const STATUS_COLORS: Record<string, string> = {
    Todo: 'bg-gray-500',
    InProgress: 'bg-yellow-500',
    Review: 'bg-purple-500',
    Done: 'bg-green-500',
};

const COMPLEXITY_COLORS: Record<string, string> = {
    Simple: 'bg-green-500',
    Moderate: 'bg-yellow-500',
    Complex: 'bg-orange-500',
    Major: 'bg-red-500',
};

const taskEditSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    deliverable: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    risks: z.string().optional(),
    requiredFollowUps: z.string().optional(),
    complexity: z.enum(['Simple', 'Moderate', 'Complex', 'Major']),
    status: z.enum(['Todo', 'InProgress', 'Review', 'Done']),
});

type TaskEditFormData = z.infer<typeof taskEditSchema>;

interface TaskDetailDrawerProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    task: Task | null;
    onTaskUpdate?: () => void;
}

export function TaskDetailDrawer({ open, onOpenChange, task, onTaskUpdate }: TaskDetailDrawerProps) {
    const [isEditing, setIsEditing] = useState(false);
    const [transitioning, setTransitioning] = useState(false);
    const [targetStatus, setTargetStatus] = useState<string>('');

    const [updateTask, { loading: updateLoading }] = useMutation<UpdateTaskMutation, UpdateTaskMutationVariables>(UPDATE_TASK, {
        client: getApolloClient(),
    });

    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
    } = useForm<TaskEditFormData>({
        resolver: zodResolver(taskEditSchema),
    });

    const handleEditOpen = useCallback(() => {
        setIsEditing(true);
    }, []);

    const handleEditClose = useCallback(() => {
        setIsEditing(false);
    }, []);

    const handleEditSubmit = async (data: TaskEditFormData) => {
        try {
            await updateTask({
                variables: {
                    id: task!.id,
                    input: {
                        title: data.title,
                        deliverable: data.deliverable || null,
                        acceptanceCriteria: data.acceptanceCriteria || null,
                        risks: data.risks || null,
                        requiredFollowUps: data.requiredFollowUps || null,
                        complexity: data.complexity,
                        status: data.status,
                        version: 1,
                    },
                },
            });
            toast.success('Task updated successfully');
            setIsEditing(false);
            onTaskUpdate?.();
        } catch (err) {
            toast.error(err instanceof Error ? err.message : 'Failed to update task');
        }
    };

    const handleStatusTransition = async () => {
        if (!targetStatus || !task) return;

        setTransitioning(true);
        try {
            await updateTask({
                variables: {
                    id: task.id,
                    input: {
                        title: task.title,
                        deliverable: task.deliverable || null,
                        acceptanceCriteria: task.acceptanceCriteria || null,
                        risks: task.risks || null,
                        requiredFollowUps: task.requiredFollowUps || null,
                        complexity: task.complexity,
                        status: targetStatus as Task['status'],
                        version: 1,
                    },
                },
            });
            toast.success('Status transition successful');
            setTargetStatus('');
            onTaskUpdate?.();
        } catch (err) {
            toast.error(err instanceof Error ? err.message : 'Failed to transition status');
        } finally {
            setTransitioning(false);
        }
    };

    const handleDrawerClose = useCallback(() => {
        setTargetStatus('');
        setIsEditing(false);
        onOpenChange(false);
    }, [onOpenChange]);

    if (!task) return null;

    const validTransitions: string[] = [];
    if (task.status === 'Todo') {
        validTransitions.push('InProgress');
    } else if (task.status === 'InProgress') {
        validTransitions.push('Review', 'Todo');
    } else if (task.status === 'Review') {
        validTransitions.push('Done', 'InProgress');
    } else if (task.status === 'Done') {
        validTransitions.push('Review');
    }

    return (
        <Drawer open={open} onOpenChange={handleDrawerClose}>
            <DrawerContent className="max-h-[90vh] overflow-y-auto">
                <DrawerHeader>
                    <DrawerTitle className="flex items-center gap-3">
                        {task.title}
                        <Badge className={STATUS_COLORS[task.status] || 'bg-gray-500'}>
                            {task.status}
                        </Badge>
                    </DrawerTitle>
                    <DrawerDescription>
                        Task details
                    </DrawerDescription>
                </DrawerHeader>

                <div className="p-4 space-y-4">
                    {task.complexity && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Complexity</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <Badge className={COMPLEXITY_COLORS[task.complexity] || 'bg-gray-500'}>
                                    {task.complexity}
                                </Badge>
                            </CardContent>
                        </Card>
                    )}

                    {task.deliverable && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Deliverable</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{task.deliverable}</p>
                            </CardContent>
                        </Card>
                    )}

                    {task.acceptanceCriteria && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Acceptance Criteria</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{task.acceptanceCriteria}</p>
                            </CardContent>
                        </Card>
                    )}

                    {task.risks && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Risks</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{task.risks}</p>
                            </CardContent>
                        </Card>
                    )}

                    {task.requiredFollowUps && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Required Follow-ups</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm whitespace-pre-wrap">{task.requiredFollowUps}</p>
                            </CardContent>
                        </Card>
                    )}

                    <Card>
                        <CardHeader>
                            <CardTitle className="text-sm">Status Transition</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="flex items-center gap-2 mb-3">
                                <span className="text-xs font-medium">Current:</span>
                                <Badge className={STATUS_COLORS[task.status] || 'bg-gray-500'}>
                                    {task.status}
                                </Badge>
                            </div>
                            {validTransitions.length > 0 ? (
                                <div className="flex gap-2">
                                    <Select value={targetStatus} onValueChange={setTargetStatus}>
                                        <SelectTrigger className="w-32">
                                            <SelectValue placeholder="Target" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            {validTransitions.map((status) => (
                                                <SelectItem key={status} value={status}>
                                                    {status}
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                    <Button
                                        onClick={handleStatusTransition}
                                        disabled={transitioning || !targetStatus}
                                        size="sm"
                                    >
                                        {transitioning ? '...' : 'Transition'}
                                    </Button>
                                </div>
                            ) : (
                                <p className="text-xs text-muted-foreground">
                                    No valid transitions available
                                </p>
                            )}
                        </CardContent>
                    </Card>

                    <div className="grid grid-cols-2 gap-4 text-sm text-muted-foreground">
                        <div>
                            <span className="font-medium">Created:</span> {new Date(task.createdAt).toLocaleDateString()}
                        </div>
                        <div>
                            <span className="font-medium">Updated:</span> {new Date(task.updatedAt).toLocaleDateString()}
                        </div>
                    </div>

                    {task.feature && (
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-sm">Parent Feature</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-sm">{task.feature.title}</p>
                            </CardContent>
                        </Card>
                    )}
                </div>

                <DrawerFooter>
                    <Button variant="outline" onClick={handleEditOpen}>
                        Edit
                    </Button>
                    <Button variant="outline" onClick={handleDrawerClose}>
                        Close
                    </Button>
                </DrawerFooter>
            </DrawerContent>

            {isEditing && (
                <Drawer open={isEditing} onOpenChange={handleEditClose}>
                    <DrawerContent className="max-h-[90vh] overflow-y-auto">
                        <DrawerHeader>
                            <DrawerTitle>Edit Task</DrawerTitle>
                        </DrawerHeader>
                        <form onSubmit={handleSubmit(handleEditSubmit)}>
                            <div className="p-4 space-y-4">
                                <div className="grid gap-2">
                                    <Label htmlFor="edit-title">Title</Label>
                                    <Input
                                        id="edit-title"
                                        {...register('title')}
                                    />
                                    {errors.title && (
                                        <p className="text-sm text-destructive">{errors.title.message}</p>
                                    )}
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-deliverable">Deliverable</Label>
                                    <Textarea
                                        id="edit-deliverable"
                                        {...register('deliverable')}
                                        rows={3}
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-acceptanceCriteria">Acceptance Criteria</Label>
                                    <Textarea
                                        id="edit-acceptanceCriteria"
                                        {...register('acceptanceCriteria')}
                                        rows={3}
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-risks">Risks</Label>
                                    <Textarea
                                        id="edit-risks"
                                        {...register('risks')}
                                        rows={2}
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-requiredFollowUps">Required Follow-ups</Label>
                                    <Textarea
                                        id="edit-requiredFollowUps"
                                        {...register('requiredFollowUps')}
                                        rows={2}
                                    />
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-complexity">Complexity</Label>
                                    <Select onValueChange={(value) => setValue('complexity', value as TaskComplexity)}>
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Simple">Simple</SelectItem>
                                            <SelectItem value="Moderate">Moderate</SelectItem>
                                            <SelectItem value="Complex">Complex</SelectItem>
                                            <SelectItem value="Major">Major</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div className="grid gap-2">
                                    <Label htmlFor="edit-status">Status</Label>
                                    <Select onValueChange={(value) => setValue('status', value as TaskStatus)}>
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Todo">Todo</SelectItem>
                                            <SelectItem value="InProgress">InProgress</SelectItem>
                                            <SelectItem value="Review">Review</SelectItem>
                                            <SelectItem value="Done">Done</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>
                            </div>
                            <DrawerFooter>
                                <Button type="submit" disabled={updateLoading}>
                                    {updateLoading ? 'Saving...' : 'Save Changes'}
                                </Button>
                                <Button type="button" variant="outline" onClick={handleEditClose}>
                                    Cancel
                                </Button>
                            </DrawerFooter>
                        </form>
                    </DrawerContent>
                </Drawer>
            )}
        </Drawer>
    );
}
