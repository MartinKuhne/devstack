import { test, expect } from '@playwright/test';
import { DashboardPage } from './pages/DashboardPage.js';

test.describe('Dashboard Page', () => {
    let dashboardPage: DashboardPage;

    test.beforeEach(async ({ page }) => {
        dashboardPage = new DashboardPage(page);
    });

    test('navigates to dashboard page', async ({ page }) => {
        await dashboardPage.navigate();
        await expect(dashboardPage.pageTitle).toBeVisible();
        await expect(dashboardPage.welcomeText).toBeVisible();
    });

    test('renders summary cards with data', async ({ page }) => {
        await page.route('**/graphql', async (route) => {
            await route.fulfill({
                status: 200,
                body: JSON.stringify({
                    data: {
                        dashboardSummary: {
                            projectsInFlight: 1,
                            featuresInReview: 2,
                            featuresFailed: 0,
                            tasksInProgress: 3,
                            tasksFailed: 0,
                            recentAuditEvents: [],
                            __typename: 'DashboardSummary',
                        },
                    },
                }),
            });
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.projectsInFlightCard).toBeVisible();
        await expect(dashboardPage.featuresInReviewCard).toBeVisible();
        await expect(dashboardPage.tasksInProgressCard).toBeVisible();
    });

    test('shows empty state when no data exists', async ({ page }) => {
        await page.route('**/graphql', async (route) => {
            await route.fulfill({
                status: 200,
                body: JSON.stringify({
                    data: {
                        dashboardSummary: {
                            projectsInFlight: 0,
                            featuresInReview: 0,
                            featuresFailed: 0,
                            tasksInProgress: 0,
                            tasksFailed: 0,
                            recentAuditEvents: [],
                            __typename: 'DashboardSummary',
                        },
                    },
                }),
            });
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        await expect(dashboardPage.emptyStateMessage).toBeVisible();
    });
});
