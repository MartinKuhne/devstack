import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasks');

const POLL_INTERVAL_MS = 5000;

/**
 * Fetches agent tasks for a specific deliverable with optional client-side status filtering.
 * Polls every 5s.
 */
export function useAgentTasks(deliverableId: string, statusFilter?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { deliverableId },
        fetchPolicy: 'cache-and-network',
        skip: !deliverableId,
        pollInterval: POLL_INTERVAL_MS,
    });

    const allTasks = data?.agentTasks?.nodes ?? [];

    const filteredTasks =
        statusFilter && statusFilter.length > 0
            ? allTasks.filter(
                  (task): task is NonNullable<typeof task> =>
                      task !== null && statusFilter.includes(task.status as AgentTaskStatus)
              )
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
