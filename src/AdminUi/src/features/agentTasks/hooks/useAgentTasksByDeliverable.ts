import type { AgentTaskStatus } from '@/generated/graphql';
import { useGetAgentTasksQuery } from '@/generated/graphql';

export function useAgentTasksByDeliverable(deliverableId?: string, statusFilter?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { projectId: deliverableId ?? null },
        fetchPolicy: 'cache-and-network',
        skip: !deliverableId,
    });

    const allTasks = data?.agentTasks ?? [];
    
    const filteredByDeliverable = allTasks.filter(task => task.deliverableId === deliverableId);
    
    const filteredTasks = statusFilter && statusFilter.length > 0
        ? filteredByDeliverable.filter(task => statusFilter.includes(task.status as AgentTaskStatus))
        : filteredByDeliverable;

    return {
        agentTasks: filteredTasks,
        loading,
        error,
        refetch,
    };
}
