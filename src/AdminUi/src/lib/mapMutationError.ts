import { toast } from 'react-toastify';
import { createModuleLogger } from './logging';

const logger = createModuleLogger('mapMutationError');

export function mapMutationError(error: unknown, context?: string): string {
    if (error instanceof Error) {
        const message = error.message;
        if (message.includes('CONCURRENCY_CONFLICT')) {
            const label = context ?? 'item';
            return `The ${label} was modified by another user. Please refresh and try again.`;
        }
        if (message.includes('NOT_FOUND')) {
            return `${context ?? 'Item'} not found. It may have been deleted.`;
        }
        return message;
    }
    return 'An unexpected error occurred.';
}

export function toastMutationError(error: unknown, context?: string): void {
    const message = mapMutationError(error, context);
    logger.error('Mutation error', { message, context });
    toast.error(message);
}
