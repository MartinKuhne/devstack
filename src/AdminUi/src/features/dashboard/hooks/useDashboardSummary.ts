import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import { GetDashboardSummaryDocument } from '@/graphql/queries/dashboard.graphql';
import type { GetDashboardSummaryQuery } from '@/generated/graphql';

export function useDashboardSummary() {
    const { data, loading, error, refetch } = useQuery<GetDashboardSummaryQuery>(GetDashboardSummaryDocument, {
        client: getApolloClient(),
        fetchPolicy: 'cache-and-network',
    });

    return {
        dashboardSummary: data?.dashboardSummary,
        loading,
        error,
        refetch,
    };
}
