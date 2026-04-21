import { test, expect } from '@playwright/test';
import { DashboardPage } from '../pages/DashboardPage.js';
import { NavigationHelper } from '../helpers/NavigationHelper.js';

test.describe('Dashboard', () => {
    let dashboardPage: DashboardPage;
    let navigationHelper: NavigationHelper;

    test.beforeEach(async ({ page }) => {
        dashboardPage = new DashboardPage(page);
        navigationHelper = new NavigationHelper(page);
    });

    test('should display dashboard with correct heading', async () => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        await expect(dashboardPage.pageTitle).toBeVisible();
    });

    test('should show welcome message', async () => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        await expect(dashboardPage.welcomeText).toBeVisible();
    });

    test('should display stat cards for deliverable counts', async ({ page }) => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        
        // All four stat card labels should be visible (Planning, Ready, In Progress, Needs Review)
        await expect(page.getByText('Planning')).toBeVisible();
        await expect(page.getByText('Ready')).toBeVisible();
        await expect(page.getByText('In Progress')).toBeVisible();
        await expect(page.getByText('Needs Review')).toBeVisible();
    });

    test('should have New Project button', async () => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        await expect(dashboardPage.newProjectButton).toBeVisible();
    });

    test('should navigate to projects when clicking sidebar Projects link', async ({ page }) => {
        await dashboardPage.navigate();
        await navigationHelper.navigateToProjects();
        await expect(page.getByRole('heading', { name: 'Projects', level: 2 })).toBeVisible();
    });

    test('should navigate to models when clicking sidebar Models link', async ({ page }) => {
        await dashboardPage.navigate();
        await navigationHelper.navigateToModels();
        await expect(page.getByRole('heading', { name: 'Large Language Models', level: 2 })).toBeVisible();
    });
});
