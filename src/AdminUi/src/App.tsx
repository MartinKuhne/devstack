import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';
import { createModuleLogger } from './lib/logging';

const routeLogger = createModuleLogger('App');

function LoadingFallback() {
    return (
        <div className="flex items-center justify-center p-8">
            <div className="text-muted-foreground">Loading...</div>
        </div>
    );
}

function NotFoundPage() {
    return (
        <div className="flex items-center justify-center min-h-[60vh]">
            <div className="text-center">
                <h1 className="text-4xl font-bold text-destructive mb-4">404</h1>
                <h2 className="text-2xl font-semibold mb-2">Page Not Found</h2>
                <p className="text-muted-foreground mb-4">
                    The page you are looking for does not exist.
                </p>
                <a href="/" className="text-primary underline hover:text-primary/80">
                    Go back to Dashboard
                </a>
            </div>
        </div>
    );
}

const DashboardPage = lazy(() => {
    routeLogger.debug('Lazy loading DashboardPage');
    return import('./features/dashboard/pages/DashboardPage').then((module) => {
        routeLogger.debug('DashboardPage loaded');
        return { default: module.DashboardPage };
    });
});
const ProjectListPage = lazy(() => {
    routeLogger.debug('Lazy loading ProjectListPage');
    return import('./features/projects/pages/ProjectListPage').then((module) => {
        routeLogger.debug('ProjectListPage loaded');
        return { default: module.ProjectListPage };
    });
});
const ProjectDetailPage = lazy(() => {
    routeLogger.debug('Lazy loading ProjectDetailPage');
    return import('./features/projects/pages/ProjectDetailPage').then((module) => {
        routeLogger.debug('ProjectDetailPage loaded');
        return { default: module.ProjectDetailPage };
    });
});
const DeliverableListPage = lazy(() => {
    routeLogger.debug('Lazy loading DeliverableListPage');
    return import('./features/deliverables/pages/DeliverableListPage').then((module) => {
        routeLogger.debug('DeliverableListPage loaded');
        return { default: module.DeliverableListPage };
    });
});
const DeliverableDetailPage = lazy(() => {
    routeLogger.debug('Lazy loading DeliverableDetailPage');
    return import('./features/deliverables/pages/DeliverableDetailPage').then((module) => {
        routeLogger.debug('DeliverableDetailPage loaded');
        return { default: module.DeliverableDetailPage };
    });
});
const AgentTaskListPage = lazy(() => {
    routeLogger.debug('Lazy loading AgentTaskListPage');
    return import('./features/agentTasks/pages/AgentTaskListPage').then((module) => {
        routeLogger.debug('AgentTaskListPage loaded');
        return { default: module.AgentTaskListPage };
    });
});
const AgentTaskDetailPage = lazy(() => {
    routeLogger.debug('Lazy loading AgentTaskDetailPage');
    return import('./features/agentTasks/pages/AgentTaskDetailPage').then((module) => {
        routeLogger.debug('AgentTaskDetailPage loaded');
        return { default: module.AgentTaskDetailPage };
    });
});
const LargeLanguageModelsPage = lazy(() => {
    routeLogger.debug('Lazy loading LargeLanguageModelsPage');
    return import('./features/largeLanguageModels/pages/LargeLanguageModelsPage').then((module) => {
        routeLogger.debug('LargeLanguageModelsPage loaded');
        return { default: module.LargeLanguageModelsPage };
    });
});

function DashboardPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Dashboard">
            <Suspense fallback={<LoadingFallback />}>
                <DashboardPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function ProjectListPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Projects">
            <Suspense fallback={<LoadingFallback />}>
                <ProjectListPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function ProjectDetailPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Project Detail">
            <Suspense fallback={<LoadingFallback />}>
                <ProjectDetailPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function DeliverableListPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Deliverables">
            <Suspense fallback={<LoadingFallback />}>
                <DeliverableListPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function DeliverableDetailPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Deliverable Detail">
            <Suspense fallback={<LoadingFallback />}>
                <DeliverableDetailPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function AgentTaskListPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Agent Tasks">
            <Suspense fallback={<LoadingFallback />}>
                <AgentTaskListPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function AgentTaskDetailPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Agent Task Detail">
            <Suspense fallback={<LoadingFallback />}>
                <AgentTaskDetailPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function LargeLanguageModelsPageWithErrorBoundary() {
    return (
        <ErrorBoundary name="Large Language Models">
            <Suspense fallback={<LoadingFallback />}>
                <LargeLanguageModelsPage />
            </Suspense>
        </ErrorBoundary>
    );
}

function App() {
    return (
        <BrowserRouter>
            <ErrorBoundary>
                <Routes>
                    <Route path="/" element={<AppShell />}>
                        <Route index element={<DashboardPageWithErrorBoundary />} />
                        <Route path="projects" element={<ProjectListPageWithErrorBoundary />} />
                        <Route
                            path="projects/:id"
                            element={<ProjectDetailPageWithErrorBoundary />}
                        />
                        <Route
                            path="deliverables"
                            element={<DeliverableListPageWithErrorBoundary />}
                        />
                        <Route
                            path="deliverables/:id"
                            element={<DeliverableDetailPageWithErrorBoundary />}
                        />
                        <Route
                            path="agent-tasks"
                            element={<AgentTaskListPageWithErrorBoundary />}
                        />
                        <Route
                            path="agent-tasks/:id"
                            element={<AgentTaskDetailPageWithErrorBoundary />}
                        />
                        <Route
                            path="models"
                            element={<LargeLanguageModelsPageWithErrorBoundary />}
                        />
                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
