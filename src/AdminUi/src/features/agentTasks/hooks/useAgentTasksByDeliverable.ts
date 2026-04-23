import type { AgentTaskStatus } from '@/generated/graphql';
import { useGetAgentTasksQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasksByDeliverable');

export function useAgentTasksByDeliverable(
    _deliverableId?: string,
    statusFilter?: AgentTaskStatus[]
) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        fetchPolicy: 'cache-and-network',
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
        logger.debug('Loaded agent tasks by deliverable', {
            deliverableId: _deliverableId,
            total: allTasks.length,
            filtered: filteredTasks.length,
        });
    }

    if (error) {
        logger.error('Failed to fetch agent tasks by deliverable', {
            deliverableId: _deliverableId,
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
