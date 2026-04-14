import { gql } from '@apollo/client/react'

export const GetDashboardSummaryDocument = gql`
  query GetDashboardSummary {
    dashboardSummary {
      projectCount
      featureCount
      defectCount
      taskCount
    }
  }
`
