import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasks');

/**
 * Fetches agent tasks with optional filtering by status.
 */
export function useAgentTasks(statusFilter?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { projectId: null },
        fetchPolicy: 'cache-and-network',
    });

    const allTasks = data?.agentTasks ?? [];

    const filteredTasks =
        statusFilter && statusFilter.length > 0
            ? allTasks.filter((task) => statusFilter.includes(task.status as AgentTaskStatus))
            : allTasks;

    if (!loading && allTasks.length > 0) {
        logger.debug('Loaded agent tasks', {
            total: allTasks.length,
            filtered: filteredTasks.length,
            statusFilter,
        });
    }

    if (error) {
        logger.error('Failed to fetch agent tasks', {
            message: error.message,
        });
    }

    return {
        agentTasks: filteredTasks,
        loading,
        error,
        refetch,
    };
}
