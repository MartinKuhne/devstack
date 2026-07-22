import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Clock } from 'lucide-react';

export interface ActivityEvent {
    id: string;
    timestamp: string;
    actor: string;
    fromStatus?: string;
    toStatus: string;
    label?: string;
}

export interface ActivityTimelineProps {
    events: ActivityEvent[];
    title?: string;
}

export function ActivityTimeline({ events, title = 'Activity' }: ActivityTimelineProps) {
    return (
        <Card>
            <CardHeader>
                <CardTitle>{title}</CardTitle>
            </CardHeader>
            <CardContent>
                {events.length === 0 ? (
                    <div className="flex items-center gap-2 text-sm text-muted-foreground py-4">
                        <Clock className="h-4 w-4" />
                        <span>No activity recorded yet.</span>
                    </div>
                ) : (
                    <ol className="relative border-l border-border ml-3 space-y-4">
                        {events.map((event) => (
                            <li key={event.id} className="ml-4">
                                <div className="absolute -left-1.5 mt-1.5 h-3 w-3 rounded-full border border-border bg-background" />
                                <div className="flex items-center gap-2 mb-1">
                                    <span className="text-sm font-medium">{event.actor}</span>
                                    <span className="text-xs text-muted-foreground">
                                        {new Date(event.timestamp).toLocaleString()}
                                    </span>
                                </div>
                                <p className="text-sm text-muted-foreground">
                                    {event.fromStatus
                                        ? `changed status from ${event.fromStatus} to ${event.toStatus}`
                                        : event.label ?? `set status to ${event.toStatus}`}
                                </p>
                            </li>
                        ))}
                    </ol>
                )}
            </CardContent>
        </Card>
    );
}
