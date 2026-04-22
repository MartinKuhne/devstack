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
                variables: { input: { id } },
            });

            if (result.data?.deleteDeliverable?.errors?.length) {
                const errorMessages = result.data.deleteDeliverable.errors.map(
                    (e) => e.message
                );
                logger.warn('Failed to delete deliverable', {
                    id,
                    errors: errorMessages,
                });
                return { success: false, errors: errorMessages };
            }

            logger.info('Deliverable deleted successfully', { id });
            return { success: true };
        } catch (err) {
            logApolloError(err);
            return { success: false, errors: [err instanceof Error ? err.message : 'Failed to delete deliverable'] };
        }
    };

    return {
        deleteDeliverable: executeDelete,
        loading,
        error,
    };
}
