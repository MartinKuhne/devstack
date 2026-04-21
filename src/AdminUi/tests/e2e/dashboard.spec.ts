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
                        items: {
                            nodes: [
                                { id: '1', status: 'PLANNING', title: 'Deliverable 1', __typename: 'Deliverable' },
                                { id: '2', status: 'READY', title: 'Deliverable 2', __typename: 'Deliverable' },
                                { id: '3', status: 'IN_PROGRESS', title: 'Deliverable 3', __typename: 'Deliverable' },
                                { id: '4', status: 'NEEDS_REVIEW', title: 'Deliverable 4', __typename: 'Deliverable' },
                            ],
                        },
                    },
                }),
            });
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.deliverablesPlanningCard).toBeVisible();
        await expect(dashboardPage.deliverablesReadyCard).toBeVisible();
        await expect(dashboardPage.deliverablesInProgressCard).toBeVisible();
        await expect(dashboardPage.deliverablesNeedsReviewCard).toBeVisible();
    });

    test('shows empty state when no data exists', async ({ page }) => {
        await page.route('**/graphql', async (route) => {
            await route.fulfill({
                status: 200,
                body: JSON.stringify({
                    data: {
                        items: {
                            nodes: [],
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
