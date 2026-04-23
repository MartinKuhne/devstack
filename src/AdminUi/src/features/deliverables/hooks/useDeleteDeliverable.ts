import { useDeleteDeliverableMutation } from '@/generated/graphql';
import { logApolloError } from '@/hooks/useApolloClient';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useDeleteDeliverable');

export function useDeleteDeliverable() {
    const [deleteDeliverable, { loading, error }] = useDeleteDeliverableMutation();

    const executeDelete = async (id: string) => {
        logger.info('Deleting deliverable', { id });

        try {
            const result = await deleteDeliverable({
                variables: { id },
            });

            const deleted = result.data?.deleteDeliverable;
            if (!deleted) {
                logger.warn('Failed to delete deliverable', { id });
                return { success: false, errors: ['Failed to delete deliverable'] };
            }

            logger.info('Deliverable deleted successfully', { id });
            return { success: true };
        } catch (err) {
            logApolloError(err);
            return {
                success: false,
                errors: [err instanceof Error ? err.message : 'Failed to delete deliverable'],
            };
        }
    };

    return {
        deleteDeliverable: executeDelete,
        loading,
        error,
    };
}
