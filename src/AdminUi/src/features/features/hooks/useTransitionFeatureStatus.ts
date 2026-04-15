import { useTransitionFeatureStatusMutation } from '@/generated/graphql';

export function useTransitionFeatureStatus() {
    return useTransitionFeatureStatusMutation();
}
