import { gql } from '@apollo/client';

export const GetDashboardSummaryDocument = gql`
    query GetDashboardSummary {
        dashboardSummary {
            projectCount
            featureCount
            defectCount
            taskCount
        }
    }
`;
