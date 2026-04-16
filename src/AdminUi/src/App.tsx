import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';

const DashboardPage = lazy(() => import('./features/features/pages/DashboardPage').then(module => ({ default: module.DashboardPage })));
const ProjectListPage = lazy(() => import('./features/features/pages/ProjectListPage').then(module => ({ default: module.ProjectListPage })));
const ProjectDetailPage = lazy(() => import('./features/projects/pages/ProjectDetailPage').then(module => ({ default: module.ProjectDetailPage })));
const FeatureListPage = lazy(() => import('./features/features/pages/FeatureListPage').then(module => ({ default: module.FeatureListPage })));
const FeatureDetailPage = lazy(() => import('./features/features/pages/FeatureDetailPage').then(module => ({ default: module.FeatureDetailPage })));
const DefectListPage = lazy(() => import('./features/features/pages/DefectListPage').then(module => ({ default: module.DefectListPage })));
const DefectDetailPage = lazy(() => import('./features/defects/pages/DefectDetailPage').then(module => ({ default: module.DefectDetailPage })));
const TaskListPage = lazy(() => import('./features/features/pages/TaskListPage').then(module => ({ default: module.TaskListPage })));
const SettingsPage = lazy(() => import('./features/features/pages/SettingsPage').then(module => ({ default: module.SettingsPage })));
const EpicListPage = lazy(() => import('./features/epics/pages/EpicListPage').then(module => ({ default: module.EpicListPage })));

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
                        <Route path="features" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <FeatureListPage />
                            </Suspense>
                        } />
                        <Route path="features/:id" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <FeatureDetailPage />
                            </Suspense>
                        } />
                        <Route path="defects" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <DefectListPage />
                            </Suspense>
                        } />
                        <Route path="defects/:id" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <DefectDetailPage />
                            </Suspense>
                        } />
                        <Route path="tasks" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <TaskListPage />
                            </Suspense>
                        } />
                        <Route path="settings" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <SettingsPage />
                            </Suspense>
                        } />
                        <Route path="epics" element={
                            <Suspense fallback={<LoadingFallback />}>
                                <EpicListPage />
                            </Suspense>
                        } />
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
