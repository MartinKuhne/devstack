import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';

export function useAgentTasksByDeliverable(deliverableId?: string, status?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { itemId: deliverableId, status: status ?? null },
        fetchPolicy: 'cache-and-network',
        skip: !deliverableId,
    });

    const allAgentTasks = data?.agentTasks?.nodes ?? [];
    const filteredAgentTasks = allAgentTasks.filter(task => task.deliverable === deliverableId);

    return {
        agentTasks: filteredAgentTasks,
        loading,
        error,
        refetch,
    };
}
