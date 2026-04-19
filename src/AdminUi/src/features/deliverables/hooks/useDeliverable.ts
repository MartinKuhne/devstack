import { useGetDeliverableByIdQuery } from '@/generated/graphql';

export function useDeliverable(id: string) {
    const { data, loading, error, refetch } = useGetDeliverableByIdQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    return {
        deliverable: data?.getItemById ?? null,
        loading,
        error,
        refetch,
    };
}
