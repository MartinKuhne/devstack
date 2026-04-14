import { useState, useCallback } from 'react';
import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import { GetDashboardSummaryDocument } from '@/graphql/queries/dashboard.graphql';
import type { GetDashboardSummaryQuery } from '@/generated/graphql';

export function useDashboardSummary() {
    const [isBackgroundRefresh, setIsBackgroundRefresh] = useState(false);
    
    const { data, loading, error, refetch } = useQuery<GetDashboardSummaryQuery>(GetDashboardSummaryDocument, {
        client: getApolloClient(),
        fetchPolicy: 'cache-and-network',
        pollInterval: 30000,
    });

    const handleRefetch = useCallback(async () => {
        setIsBackgroundRefresh(true);
        try {
            await refetch();
        } finally {
            setTimeout(() => setIsBackgroundRefresh(false), 1000);
        }
    }, [refetch]);

    return {
        dashboardSummary: data?.dashboardSummary,
        loading,
        error,
        refetch: handleRefetch,
        isBackgroundRefresh,
    };
}
