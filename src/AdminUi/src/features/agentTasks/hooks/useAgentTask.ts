import { useGetAgentTaskQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTask');

export function useAgentTask(id: string) {
    const { data, loading, error, refetch } = useGetAgentTaskQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    const agentTask = data?.agentTask ?? null;

    if (!loading && !agentTask && !error && id) {
        logger.warn('Agent task not found', { id });
    }

    if (error) {
        logger.error('Failed to fetch agent task', {
            id,
            message: error.message,
        });
    }

    if (!loading && agentTask) {
        logger.debug('Loaded agent task', { id: agentTask.id, title: agentTask.title });
    }

    return {
        agentTask,
        loading,
        error,
        refetch,
    };
}
