import { useNavigate } from 'react-router';
import {
    PageHeader,
    EmptyState,
} from '@/components/layout';

export function AgentTaskListPage() {
    const navigate = useNavigate();

    return (
        <div className="space-y-6">
            <PageHeader
                title="Agent Tasks"
                description="Manage agent task execution and telemetry."
            />

            <EmptyState
                description="Navigate to a deliverable to view its agent tasks."
                action={{ label: 'View Deliverables', onClick: () => navigate('/deliverables') }}
            />
        </div>
    );
}
