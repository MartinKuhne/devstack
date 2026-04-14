import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Drawer, DrawerContent, DrawerHeader, DrawerTitle, DrawerDescription, DrawerFooter } from '@/components/ui/drawer';
import { Button } from '@/components/ui/button';
import type { Task } from '@/generated/graphql';

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

interface TaskDetailDrawerProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    task: Task | null;
}

export function TaskDetailDrawer({ open, onOpenChange, task }: TaskDetailDrawerProps) {
    if (!task) return null;

    return (
        <Drawer open={open} onOpenChange={onOpenChange}>
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

                    <div className="grid grid-cols-2 gap-4 text-sm text-muted-foreground">
                        <div>
                            <span className="font-medium">Created:</span> {new Date(task.createdAt).toLocaleDateString()}
                        </div>
                        <div>
                            <span className="font-medium">Updated:</span> {new Date(task.updatedAt).toLocaleDateString()}
                        </div>
                    </div>
                </div>

                <DrawerFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        Close
                    </Button>
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    );
}
