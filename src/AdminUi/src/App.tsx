import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';

const DashboardPage = lazy(() => import('./features/dashboard/pages/DashboardPage').then(module => ({ default: module.DashboardPage })));
const ProjectListPage = lazy(() => import('./features/projects/pages/ProjectListPage').then(module => ({ default: module.ProjectListPage })));
const ProjectDetailPage = lazy(() => import('./features/projects/pages/ProjectDetailPage').then(module => ({ default: module.ProjectDetailPage })));
const DeliverableListPage = lazy(() => import('./features/deliverables/pages/DeliverableListPage').then(module => ({ default: module.DeliverableListPage })));
const AgentTaskListPage = lazy(() => import('./features/agentTasks/pages/AgentTaskListPage').then(module => ({ default: module.AgentTaskListPage })));
const AgentTaskDetailPage = lazy(() => import('./features/agentTasks/pages/AgentTaskDetailPage').then(module => ({ default: module.AgentTaskDetailPage })));
const LargeLanguageModelsPage = lazy(() => import('./features/largeLanguageModels/pages/LargeLanguageModelsPage').then(module => ({ default: module.LargeLanguageModelsPage })));
const DeliverableDetailPage = lazy(() => import('./features/deliverables/pages/DeliverableDetailPage').then(module => ({ default: module.DeliverableDetailPage })));

function LoadingFallback() {
    return (
        <div className="flex items-center justify-center p-8">
            <div className="text-muted-foreground">Loading...</div>
        </div>
    );
}

function App() {
    return (
        <BrowserRouter>
            <ErrorBoundary>
                <Routes>
                    <Route path="/" element={<AppShell />}>
                        <Route index element={
                            <Suspense fallback={<LoadingFallback />}>
                                <DashboardPage />
                            </Suspense>
                        } />
                        <Route path="projects" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <ProjectListPage />
                            </Suspense>
                        } />
                        <Route path="projects/:id" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <ProjectDetailPage />
                            </Suspense>
                        } />
                        <Route path="deliverables" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <DeliverableListPage />
                            </Suspense>
                        } />
                        <Route path="deliverables/:id" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <DeliverableDetailPage />
                            </Suspense>
                        } />
                        <Route path="agent-tasks" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <AgentTaskListPage />
                            </Suspense>
                        } />
                        <Route path="agent-tasks/:id" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <AgentTaskDetailPage />
                            </Suspense>
                        } />
                        <Route path="models" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <LargeLanguageModelsPage />
                            </Suspense>
                        } />
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
