import { useGetAgentTaskByIdQuery } from '@/generated/graphql';

export function useAgentTask(id: string) {
    const { data, loading, error, refetch } = useGetAgentTaskByIdQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    return {
        agentTask: data?.agentTaskById ?? null,
        loading,
        error,
        refetch,
    };
}
