import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ApolloWrapper } from './components/ApolloWrapper.tsx';
import './index.css';
import App from './App.tsx';
import { setupGlobalErrorHandlers } from './lib/logging';

setupGlobalErrorHandlers();

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ApolloWrapper>
            <div className="sr-only">
                <a
                    href="#main-content"
                    className="bg-primary text-primary-foreground px-4 py-2 m-4 rounded"
                >
                    Skip to main content
                </a>
            </div>
            <App />
        </ApolloWrapper>
    </StrictMode>
);
