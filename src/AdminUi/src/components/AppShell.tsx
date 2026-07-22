import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { Menu, LayoutDashboard, Folder, Brain, Cpu, GitBranch, Sun, Moon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet';
import { useEffect, useState } from 'react';
import { Header } from '@/components/Header';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { useProjects } from '@/features/projects/hooks/useProjects';
import { createModuleLogger } from '@/lib/logging';
import { useProjectContext } from '@/contexts/ProjectContext';
import { useAgentTasks } from '@/features/agentTasks/hooks/useAgentTasks';
import { AgentTaskStatus } from '@/generated/graphql';

const logger = createModuleLogger('AppShell');

function getInitialDarkMode() {
    if (typeof window !== 'undefined') {
        const stored = localStorage.getItem('theme');
        if (stored) {
            return stored === 'dark';
        }
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
    return false;
}

function SidebarContent() {
    const location = useLocation();
    const navigate = useNavigate();
    const { projects, loading } = useProjects();
    const { projectId, setProjectId } = useProjectContext();

    const { agentTasks: attentionTasks } = useAgentTasks(
        undefined,
        [AgentTaskStatus.NEEDS_REVIEW, AgentTaskStatus.FAILED]
    );
    const attentionCount = attentionTasks.length;

    const isActive = (path: string) => {
        if (path === '/') return location.pathname === '/';
        return location.pathname.startsWith(path);
    };

    const handleProjectSelect = (value: string) => {
        logger.debug('Project selection changed', { projectId: value });
        if (value === 'all') {
            setProjectId('');
            navigate('/projects');
        } else {
            setProjectId(value);
            navigate(`/projects/${value}`);
        }
    };

    const hasProject = !!projectId;

    return (
        <nav className="p-4 space-y-2">
            <Select value={projectId || 'all'} onValueChange={handleProjectSelect}>
                <SelectTrigger className="w-full">
                    <Folder className="mr-2 h-4 w-4" />
                    <SelectValue placeholder="Select Project" />
                </SelectTrigger>
                <SelectContent>
                    {loading ? (
                        <SelectItem value="loading" disabled>
                            Loading...
                        </SelectItem>
                    ) : projects.length === 0 ? (
                        <SelectItem value="none" disabled>
                            No projects
                        </SelectItem>
                    ) : (
                        <>
                            <SelectItem value="all">All Projects</SelectItem>
                            {projects.map((project) => project ? (
                                <SelectItem key={project.id ?? ''} value={project.id ?? ''}>
                                    {project.name ?? 'Unnamed Project'}
                                </SelectItem>
                            ) : null)}
                        </>
                    )}
                </SelectContent>
            </Select>

            <Link to="/">
                <Button variant="ghost" className={`w-full justify-start ${isActive('/') ? 'bg-accent text-accent-foreground' : ''}`}>
                    <LayoutDashboard className="mr-2 h-4 w-4" />
                    Dashboard
                </Button>
            </Link>

            <Link to="/models">
                <Button variant="ghost" className={`w-full justify-start ${isActive('/models') ? 'bg-accent text-accent-foreground' : ''}`}>
                    <Cpu className="mr-2 h-4 w-4" />
                    Large Language Models
                </Button>
            </Link>

            <Link to="/projects">
                <Button variant="ghost" className={`w-full justify-start ${isActive('/projects') ? 'bg-accent text-accent-foreground' : ''}`}>
                    <Folder className="mr-2 h-4 w-4" />
                    Projects
                </Button>
            </Link>

            {hasProject ? (
                <Link to={`/deliverables?project=${projectId}`}>
                    <Button variant="ghost" className={`w-full justify-start ${isActive('/deliverables') ? 'bg-accent text-accent-foreground' : ''}`}>
                        <GitBranch className="mr-2 h-4 w-4" />
                        Deliverables
                    </Button>
                </Link>
            ) : (
                <Button
                    variant="ghost"
                    className="w-full justify-start opacity-50 cursor-not-allowed"
                    disabled
                    aria-label="Deliverables - select a project first"
                >
                    <GitBranch className="mr-2 h-4 w-4" />
                    Deliverables
                </Button>
            )}

            {hasProject ? (
                <Link to="/agent-tasks">
                    <Button variant="ghost" className={`w-full justify-start ${isActive('/agent-tasks') ? 'bg-accent text-accent-foreground' : ''}`}>
                        <Brain className="mr-2 h-4 w-4" />
                        Agent Tasks
                        {attentionCount > 0 && (
                            <Badge variant="destructive" className="ml-auto text-xs">
                                {attentionCount}
                            </Badge>
                        )}
                    </Button>
                </Link>
            ) : (
                <Button
                    variant="ghost"
                    className="w-full justify-start opacity-50 cursor-not-allowed"
                    disabled
                    aria-label="Agent Tasks - select a project first"
                >
                    <Brain className="mr-2 h-4 w-4" />
                    Agent Tasks
                </Button>
            )}
        </nav>
    );
}

export function AppShell() {
    const [darkMode, setDarkMode] = useState(getInitialDarkMode);

    useEffect(() => {
        const root = window.document.documentElement;
        if (darkMode) {
            root.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            root.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
    }, [darkMode]);

    useEffect(() => {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handlechange = (e: MediaQueryListEvent) => {
            const stored = localStorage.getItem('theme');
            if (!stored) {
                setDarkMode(e.matches);
                const root = window.document.documentElement;
                if (e.matches) {
                    root.classList.add('dark');
                } else {
                    root.classList.remove('dark');
                }
            }
        };
        mediaQuery.addEventListener('change', handlechange);
        return () => mediaQuery.removeEventListener('change', handlechange);
    }, []);

    return (
        <div className="min-h-screen bg-background text-foreground">
            <a
                href="#main-content"
                className="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 z-50 bg-primary text-primary-foreground px-4 py-2 rounded-md"
            >
                Skip to content
            </a>
            <div className="flex min-h-screen w-full">
                {/* Desktop Sidebar */}
                <aside className="hidden w-64 border-r bg-background md:block relative">
                    <div className="flex h-16 items-center border-b px-6">
                        <Link to="/" className="flex items-center gap-2">
                            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                                <span className="text-sm font-bold">DS</span>
                            </div>
                            <span className="text-sm font-semibold">DevStack</span>
                        </Link>
                    </div>
                    <SidebarContent />
                    <div className="absolute bottom-4 left-4">
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => setDarkMode(!darkMode)}
                            aria-label={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}
                        >
                            {darkMode ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
                        </Button>
                    </div>
                </aside>

                {/* Mobile Sidebar */}
                <div className="flex-1 flex flex-col">
                    <div className="md:hidden border-b">
                        <div className="flex h-16 items-center justify-between px-4 md:px-6">
                            <Sheet>
                                <SheetTrigger asChild>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        aria-label="Open navigation menu"
                                    >
                                        <Menu className="h-5 w-5" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent
                                    side="left"
                                    className="w-64 p-0"
                                    aria-label="Navigation menu"
                                >
                                    <div className="flex h-16 items-center border-b px-6">
                                        <Link to="/" className="flex items-center gap-2">
                                            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                                                <span className="text-sm font-bold">DS</span>
                                            </div>
                                            <span className="text-sm font-semibold">DevStack</span>
                                        </Link>
                                    </div>
                                    <SidebarContent />
                                </SheetContent>
                            </Sheet>
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setDarkMode(!darkMode)}
                                aria-label={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}
                            >
                                {darkMode ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
                            </Button>
                        </div>
                    </div>
                    <Header />
                    <main id="main-content" className="flex-1 p-4 md:p-6">
                        <Outlet />
                    </main>
                </div>
            </div>
        </div>
    );
}
