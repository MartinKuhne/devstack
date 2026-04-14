import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { useState } from 'react';
import { toast } from 'react-toastify';

interface StatusTransitionPanelProps {
    currentStatus: string;
    validTransitions: string[];
    onTransition: (targetStatus: string) => Promise<{ success: boolean; errors?: string[] }>;
}

export function StatusTransitionPanel({
    currentStatus,
    validTransitions,
    onTransition,
}: StatusTransitionPanelProps) {
    const [targetStatus, setTargetStatus] = useState<string>('');
    const [isTransitioning, setIsTransitioning] = useState(false);

    const handleTransition = async () => {
        if (!targetStatus) {
            toast.error('Please select a target status');
            return;
        }

        setIsTransitioning(true);
        try {
            const result = await onTransition(targetStatus);
            if (result.success) {
                toast.success('Status transition successful');
                setTargetStatus('');
            } else if (result.errors) {
                result.errors.forEach((error) => toast.error(error));
            }
        } finally {
            setIsTransitioning(false);
        }
    };

    const getStatusColor = (
        status: string
    ): 'default' | 'secondary' | 'destructive' | 'outline' => {
        const colors: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
            Planning: 'default',
            Ready: 'secondary',
            InProgress: 'outline',
            InReview: 'secondary',
            ReadyForTest: 'outline',
            Testing: 'outline',
            Done: 'default',
            Failed: 'destructive',
            Rejected: 'destructive',
        };
        return colors[status] || 'default';
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>Status Transition</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="flex items-center gap-2">
                    <span className="text-sm font-medium">Current Status:</span>
                    <Badge variant={getStatusColor(currentStatus)}>{currentStatus}</Badge>
                </div>

                {validTransitions.length > 0 ? (
                    <div className="space-y-3">
                        <div className="flex gap-2 items-center">
                            <Select value={targetStatus} onValueChange={setTargetStatus}>
                                <SelectTrigger className="w-48">
                                    <SelectValue placeholder="Select target status" />
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
                                onClick={handleTransition}
                                disabled={isTransitioning || !targetStatus}
                            >
                                {isTransitioning ? 'Transitioning...' : 'Transition'}
                            </Button>
                        </div>
                    </div>
                ) : (
                    <div className="text-sm text-muted-foreground">
                        No valid status transitions available for the current status.
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
