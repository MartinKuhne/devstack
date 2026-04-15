import { useUpdateProjectMutation } from '@/generated/graphql';

export function useUpdateProject() {
    return useUpdateProjectMutation();
}
