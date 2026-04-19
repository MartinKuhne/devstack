import { Outlet, Link } from 'react-router-dom';
import { Menu, LayoutDashboard, Folder, Brain, Package, Terminal } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet';
import { useEffect, useState } from 'react';
import { Header } from '@/components/Header';

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
    return (
        <nav className="p-4 space-y-2">
            <Link to="/">
                <Button variant="ghost" className="w-full justify-start">
                    <LayoutDashboard className="mr-2 h-4 w-4" />
                    Dashboard
                </Button>
            </Link>
            <Link to="/projects">
                <Button variant="ghost" className="w-full justify-start">
                    <Folder className="mr-2 h-4 w-4" />
                    Projects
                </Button>
            </Link>
            <Link to="/deliverables">
                <Button variant="ghost" className="w-full justify-start">
                    <Package className="mr-2 h-4 w-4" />
                    Deliverables
                </Button>
            </Link>
            <Link to="/agent-tasks">
                <Button variant="ghost" className="w-full justify-start">
                    <Terminal className="mr-2 h-4 w-4" />
                    Agent Tasks
                </Button>
            </Link>
            <Link to="/models">
                <Button variant="ghost" className="w-full justify-start">
                    <Brain className="mr-2 h-4 w-4" />
                    Large Language Models
                </Button>
            </Link>
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
                <aside className="hidden w-64 border-r bg-background md:block">
                    <div className="flex h-16 items-center border-b px-6">
                        <Link to="/" className="flex items-center gap-2">
                            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                                <span className="text-sm font-bold">DS</span>
                            </div>
                            <span className="text-sm font-semibold">DevStack</span>
                        </Link>
                    </div>
                    <SidebarContent />
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
                                <SheetContent side="left" className="w-64 p-0" aria-label="Navigation menu">
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
