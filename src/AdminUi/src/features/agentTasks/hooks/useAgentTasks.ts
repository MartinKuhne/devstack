import { useGetAgentTasksQuery } from '@/generated/graphql';
import type { AgentTaskStatus } from '@/generated/graphql';

export function useAgentTasks(itemId?: string, status?: AgentTaskStatus[]) {
    const { data, loading, error, refetch } = useGetAgentTasksQuery({
        variables: { itemId, status: status ?? null },
        fetchPolicy: 'cache-and-network',
        skip: !itemId,
    });

    return {
        agentTasks: data?.agentTasks?.nodes ?? [],
        loading,
        error,
        refetch,
    };
}
