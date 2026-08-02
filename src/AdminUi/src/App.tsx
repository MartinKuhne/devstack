import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';
import { createModuleLogger } from './lib/logging';

const routeLogger = createModuleLogger('App');

function LoadingFallback() {
    return (
        <div className="flex items-center justify-center p-8">
            <div className="text-muted-foreground flex items-center gap-2">
                <div className="h-4 w-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                Loading...
            </div>
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
                <Link to="/" className="text-primary underline hover:text-primary/80">
                    Go back to Dashboard
                </Link>
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

interface LazyRouteProps {
    name: string;
    children: React.ReactNode;
}

function LazyRoute({ name, children }: LazyRouteProps) {
    return (
        <ErrorBoundary name={name}>
            <Suspense fallback={<LoadingFallback />}>{children}</Suspense>
        </ErrorBoundary>
    );
}

function App() {
    return (
        <BrowserRouter>
            <ErrorBoundary>
                <Routes>
                    <Route path="/" element={<AppShell />}>
                        <Route
                            index
                            element={
                                <LazyRoute name="Dashboard">
                                    <DashboardPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="projects"
                            element={
                                <LazyRoute name="Projects">
                                    <ProjectListPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="projects/:id"
                            element={
                                <LazyRoute name="Project Detail">
                                    <ProjectDetailPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="deliverables"
                            element={
                                <LazyRoute name="Deliverables">
                                    <DeliverableListPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="deliverables/:id"
                            element={
                                <LazyRoute name="Deliverable Detail">
                                    <DeliverableDetailPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="agent-tasks"
                            element={
                                <LazyRoute name="Agent Tasks">
                                    <AgentTaskListPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="agent-tasks/:id"
                            element={
                                <LazyRoute name="Agent Task Detail">
                                    <AgentTaskDetailPage />
                                </LazyRoute>
                            }
                        />
                        <Route
                            path="models"
                            element={
                                <LazyRoute name="Large Language Models">
                                    <LargeLanguageModelsPage />
                                </LazyRoute>
                            }
                        />
                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
