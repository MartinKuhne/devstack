import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ErrorBoundary } from './components/ErrorBoundary';
import { DashboardPage } from './features/features/pages/DashboardPage';
import { ProjectListPage } from './features/features/pages/ProjectListPage';
import { ProjectDetailPage } from './features/projects/pages/ProjectDetailPage';
import { FeatureListPage } from './features/features/pages/FeatureListPage';
import { FeatureDetailPage } from './features/features/pages/FeatureDetailPage';
import { DefectListPage } from './features/features/pages/DefectListPage';
import { DefectDetailPage } from './features/defects/pages/DefectDetailPage';
import { TaskListPage } from './features/features/pages/TaskListPage';
import { SettingsPage } from './features/features/pages/SettingsPage';

function App() {
    return (
        <BrowserRouter>
            <ErrorBoundary>
                <Routes>
                    <Route path="/" element={<AppShell />}>
                        <Route index element={<DashboardPage />} />
                        <Route path="projects" element={<ProjectListPage />} />
                        <Route path="projects/:id" element={<ProjectDetailPage />} />
                        <Route path="features" element={<FeatureListPage />} />
                        <Route path="features/:id" element={<FeatureDetailPage />} />
                        <Route path="defects" element={<DefectListPage />} />
                        <Route path="defects/:id" element={<DefectDetailPage />} />
                        <Route path="tasks" element={<TaskListPage />} />
                        <Route path="settings" element={<SettingsPage />} />
                    </Route>
                </Routes>
            </ErrorBoundary>
        </BrowserRouter>
    );
}

export default App;
