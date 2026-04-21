import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';

/**
 * Fetches agent tasks with optional filtering by status.
 */
export function useAgentTasks(statusFilter?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { projectId: null },
        fetchPolicy: 'cache-and-network',
    });

    const allTasks = data?.agentTasks ?? [];
    
    const filteredTasks = statusFilter && statusFilter.length > 0
        ? allTasks.filter(task => statusFilter.includes(task.status as AgentTaskStatus))
        : allTasks;

    return {
        agentTasks: filteredTasks,
        loading,
        error,
        refetch,
    };
}
