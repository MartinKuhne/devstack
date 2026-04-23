import { useAgentTasks } from './useAgentTasks';
import type { AgentTaskStatus } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAgentTasksByDeliverable');

/**
 * Fetches agent tasks for a specific deliverable with optional status filtering.
 */
export function useAgentTasksByDeliverable(
    deliverableId?: string,
    statusFilter?: AgentTaskStatus[]
) {
    const { agentTasks, loading, error, refetch } = useAgentTasks(deliverableId, statusFilter);

    if (!loading && agentTasks.length > 0) {
        logger.debug('Loaded agent tasks by deliverable', {
            deliverableId,
            total: agentTasks.length,
        });
    }

    if (error) {
        logger.error('Failed to fetch agent tasks by deliverable', {
            deliverableId,
            message: error.message,
        });
    }

    return {
        agentTasks,
        loading,
        error,
        refetch,
    };
}
