import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { ApolloWrapper } from './components/ApolloWrapper.tsx';
import { loadErrorMessages, loadDevMessages } from '@apollo/client/dev';
import './index.css';
import App from './App.tsx';
import { setupGlobalErrorHandlers } from './lib/logging';
import { ProjectProvider } from './contexts/ProjectContext';

if (import.meta.env.DEV) {
    loadDevMessages();
}
loadErrorMessages();

setupGlobalErrorHandlers();

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ApolloWrapper>
            <ProjectProvider>
                <App />
            </ProjectProvider>
            <ToastContainer position="top-right" autoClose={3000} />
        </ApolloWrapper>
    </StrictMode>
);
