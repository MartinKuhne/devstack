import { useState, useCallback } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { CreateTaskDialog } from '@/features/features/components/CreateTaskDialog';
import { TaskDetailDrawer } from '@/features/features/components/TaskDetailDrawer';
import type { AgentTask } from '@/generated/graphql';
import { toast } from 'react-toastify';

const STATUS_COLUMNS: { status: string; label: string }[] = [
    { status: 'READY', label: 'To Do' },
    { status: 'CODE', label: 'In Progress' },
    { status: 'REVIEW', label: 'Review' },
    { status: 'DONE', label: 'Done' },
];

const COMPLEXITY_COLORS: Record<string, string> = {
    Simple: 'bg-green-500',
    Moderate: 'bg-yellow-500',
    Complex: 'bg-orange-500',
    Major: 'bg-red-500',
};

function complexityRatingToLabel(rating: number): string {
    if (rating <= 3) return 'Simple';
    if (rating <= 5) return 'Moderate';
    if (rating <= 7) return 'Complex';
    return 'Major';
}

interface TaskBoardProps {
    tasks: AgentTask[];
    featureId: string;
    onTaskClick?: (task: AgentTask) => void;
    onTasksChange?: () => void;
}

export function TaskBoard({ tasks, featureId, onTaskClick, onTasksChange }: TaskBoardProps) {
    const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
    const [selectedTask, setSelectedTask] = useState<AgentTask | null>(null);

    const handleCreateSuccess = useCallback(() => {
        toast.success('Task created successfully');
        onTasksChange?.();
    }, [onTasksChange]);

    const handleCardClick = useCallback((task: AgentTask) => {
        setSelectedTask(task);
        onTaskClick?.(task);
    }, [onTaskClick]);

    const handleDrawerClose = useCallback(() => {
        setSelectedTask(null);
    }, []);

    const tasksByStatus = STATUS_COLUMNS.map(col => ({
        ...col,
        tasks: tasks.filter(t => t.status === col.status),
    }));

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">Task Board</h3>
                <Button onClick={() => setIsCreateDialogOpen(true)}>
                    New Task
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {tasksByStatus.map(column => (
                    <Card key={column.status} className="flex flex-col">
                        <CardContent className="p-3">
                            <div className="flex items-center justify-between mb-3">
                                <h4 className="font-medium text-sm">{column.label}</h4>
                                <Badge variant="secondary" className="text-xs">
                                    {column.tasks.length}
                                </Badge>
                            </div>
                            <ScrollArea className="h-[400px]">
                                <div className="space-y-2">
                                    {column.tasks.map(task => {
                                        const complexityLabel = complexityRatingToLabel(task.complexityRating ?? 5);
                                        return (
                                            <div
                                                key={task.id}
                                                className="p-3 border rounded-lg bg-background hover:bg-accent cursor-pointer transition-colors"
                                                onClick={() => handleCardClick(task)}
                                            >
                                                <div className="flex items-start justify-between gap-2">
                                                    <h5 className="font-medium text-sm flex-1">{task.title}</h5>
                                                    <Badge
                                                        className={COMPLEXITY_COLORS[complexityLabel] || 'bg-gray-500'}
                                                        variant="default"
                                                    >
                                                        {complexityLabel}
                                                    </Badge>
                                                </div>
                                                {task.deliverable && (
                                                    <p className="text-xs text-muted-foreground mt-2 line-clamp-2">
                                                        {task.deliverable}
                                                    </p>
                                                )}
                                            </div>
                                        );
                                    })}
                                    {column.tasks.length === 0 && (
                                        <p className="text-sm text-muted-foreground text-center py-4">
                                            No tasks
                                        </p>
                                    )}
                                </div>
                            </ScrollArea>
                        </CardContent>
                    </Card>
                ))}
            </div>

            <            CreateTaskDialog
                open={isCreateDialogOpen}
                onOpenChange={setIsCreateDialogOpen}
                itemId={featureId}
                onSuccess={handleCreateSuccess}
            />

            {selectedTask && (
                <TaskDetailDrawer
                    open={!!selectedTask}
                    onOpenChange={handleDrawerClose}
                    task={{
                        id: selectedTask.id ?? '',
                        title: selectedTask.title ?? '',
                        status: selectedTask.status ?? '',
                        deliverable: selectedTask.deliverable ?? null,
                        acceptanceCriteria: selectedTask.acceptanceCriteria ?? null,
                        risks: selectedTask.risks ?? null,
                        requiredFollowUps: selectedTask.requiredFollowUps ?? null,
                        complexityRating: selectedTask.complexityRating ?? 5,
                        result: selectedTask.result ?? null,
                        createdAt: selectedTask.createdAt ?? '',
                        updatedAt: selectedTask.updatedAt ?? '',
                    }}
                    onTaskUpdate={onTasksChange}
                />
            )}
        </div>
    );
}
