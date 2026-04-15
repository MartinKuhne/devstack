import { useState, useCallback } from 'react';
import { useGetDashboardSummaryQuery } from '@/generated/graphql';

export function useDashboardSummary() {
    const [isBackgroundRefresh, setIsBackgroundRefresh] = useState(false);

    const { data, loading, error, refetch } = useGetDashboardSummaryQuery({
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
