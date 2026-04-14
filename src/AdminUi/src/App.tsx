import { BrowserRouter, Routes, Route, lazy, Suspense } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';

const DashboardPage = lazy(() => import('./features/features/pages/DashboardPage'));
const ProjectListPage = lazy(() => import('./features/features/pages/ProjectListPage'));
const ProjectDetailPage = lazy(() => import('./features/projects/pages/ProjectDetailPage'));
const FeatureListPage = lazy(() => import('./features/features/pages/FeatureListPage'));
const FeatureDetailPage = lazy(() => import('./features/features/pages/FeatureDetailPage'));
const DefectListPage = lazy(() => import('./features/features/pages/DefectListPage'));
const DefectDetailPage = lazy(() => import('./features/defects/pages/DefectDetailPage'));
const TaskListPage = lazy(() => import('./features/features/pages/TaskListPage'));
const SettingsPage = lazy(() => import('./features/features/pages/SettingsPage'));

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
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
