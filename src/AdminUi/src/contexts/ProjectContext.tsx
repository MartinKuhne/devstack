import { createContext, useContext, useState, type ReactNode } from 'react';

interface ProjectContextValue {
    projectId: string;
    setProjectId: (id: string) => void;
}

const ProjectContext = createContext<ProjectContextValue | undefined>(undefined);

export function ProjectProvider({ children }: { children: ReactNode }) {
    const [projectId, setProjectId] = useState('');

    return (
        <ProjectContext.Provider value={{ projectId, setProjectId }}>
            {children}
        </ProjectContext.Provider>
    );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useProjectContext(): ProjectContextValue {
    const context = useContext(ProjectContext);
    if (!context) {
        throw new Error('useProjectContext must be used within a ProjectProvider');
    }
    return context;
}
