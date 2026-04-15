import { useGetFeatureByIdQuery } from '@/generated/graphql';

export function useFeature(featureId: string) {
    return useGetFeatureByIdQuery({
        variables: { id: featureId },
        fetchPolicy: 'cache-and-network',
        skip: !featureId,
    });
}
