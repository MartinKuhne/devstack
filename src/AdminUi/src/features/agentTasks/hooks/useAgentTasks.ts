import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasks');

const POLL_INTERVAL_MS = 5000;

/**
 * Fetches agent tasks with optional filtering by deliverableId and status.
 * When deliverableId is provided, only tasks for that deliverable are fetched.
 * When deliverableId is omitted, all agent tasks across all deliverables are fetched.
 * Polls every 5s when any task is IN_PROGRESS or READY.
 */
export function useAgentTasks(deliverableId?: string, statusFilter?: AgentTaskStatus[], projectId?: string) {
    const hasFilter = !!deliverableId || !!projectId;
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: {
            ...(deliverableId ? { deliverableId } : {}),
            ...(projectId ? { projectId } : {}),
        },
        fetchPolicy: 'cache-and-network',
        skip: !hasFilter,
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
