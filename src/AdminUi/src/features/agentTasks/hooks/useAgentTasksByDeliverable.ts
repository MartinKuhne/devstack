import type { AgentTaskStatus } from '@/generated/graphql';
import { useGetAgentTasksQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasksByDeliverable');

export function useAgentTasksByDeliverable(
    deliverableId?: string,
    statusFilter?: AgentTaskStatus[]
) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { projectId: deliverableId ?? null },
        fetchPolicy: 'cache-and-network',
        skip: !deliverableId,
    });

    const allTasks = data?.agentTasks ?? [];

    const filteredByDeliverable = allTasks.filter((task) => task.deliverableId === deliverableId);

    const filteredTasks =
        statusFilter && statusFilter.length > 0
            ? filteredByDeliverable.filter((task) =>
                  statusFilter.includes(task.status as AgentTaskStatus)
              )
            : filteredByDeliverable;

    if (!loading && allTasks.length > 0) {
        logger.debug('Loaded agent tasks by deliverable', {
            deliverableId,
            total: allTasks.length,
            filtered: filteredTasks.length,
        });
    }

    if (error) {
        logger.error('Failed to fetch agent tasks by deliverable', {
            deliverableId,
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
