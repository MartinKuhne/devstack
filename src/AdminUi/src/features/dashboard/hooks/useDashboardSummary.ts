import { useQuery } from '@apollo/client/react'
import { useMemo } from 'react'
import { getApolloClient } from '@/hooks/useApolloClient'
import { GetDashboardSummaryDocument } from '@/graphql/queries/dashboard.graphql'

export function useDashboardSummary() {
  const { data, loading, error, refetch } = useQuery(GetDashboardSummaryDocument, {
    client: getApolloClient(),
    fetchPolicy: 'cache-and-network'
  })

  const dashboardSummary = useMemo(() => data?.dashboardSummary, [data])

  return {
    dashboardSummary,
    loading,
    error,
    refetch
  }
}
