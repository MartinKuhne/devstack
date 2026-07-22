import { Search, Cpu, LayoutDashboard, Folder } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import {
    CommandDialog,
    CommandInput,
    CommandList,
    CommandEmpty,
    CommandGroup,
    CommandItem,
} from '@/components/ui/command';
import { useEffect, useState } from 'react';
import { useAllDeliverables } from '@/features/deliverables/hooks/useAllDeliverables';

const navigationItems = [
    { label: 'Dashboard', to: '/', icon: LayoutDashboard },
    { label: 'Projects', to: '/projects', icon: Folder },
    { label: 'Large Language Models', to: '/models', icon: Cpu },
];

function Logo() {
    return (
        <Link to="/" className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                <span className="text-sm font-bold">DS</span>
            </div>
            <span className="hidden text-sm font-semibold sm:inline-block">DevStack</span>
        </Link>
    );
}

function SearchBar() {
    const navigate = useNavigate();
    const [open, setOpen] = useState(false);
    const { deliverables } = useAllDeliverables();

    useEffect(() => {
        const down = (e: KeyboardEvent) => {
            if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
                e.preventDefault();
                setOpen(current => !current);
            }
        };
        document.addEventListener('keydown', down);
        return () => document.removeEventListener('keydown', down);
    }, []);

    return (
        <div>
            <Button
                variant="outline"
                className="relative h-9 w-64 justify-start text-muted-foreground"
                onClick={() => setOpen(current => !current)}
            >
                <Search className="mr-2 h-4 w-4" />
                Search Deliverables...
                <kbd className="pointer-events-none absolute right-2 top-2 hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
                    <span className="text-xs">Ctrl</span>K
                </kbd>
            </Button>
            <CommandDialog open={open} onOpenChange={setOpen}>
                <CommandInput placeholder="Search deliverables by title..." />
                <CommandList>
                    <CommandEmpty>No deliverables found.</CommandEmpty>
                    <CommandGroup heading="Navigation">
                        {navigationItems.map(item => (
                            <CommandItem
                                key={item.to}
                                onSelect={() => {
                                    setOpen(false);
                                    navigate(item.to);
                                }}
                            >
                                <item.icon className="mr-2 h-4 w-4" />
                                {item.label}
                            </CommandItem>
                        ))}
                    </CommandGroup>
                    {deliverables.length > 0 && (
                        <CommandGroup heading="Deliverables">
                            {deliverables.map((deliverable) => (
                                <CommandItem
                                    key={deliverable.id ?? ''}
                                    value={deliverable.title ?? ''}
                                    onSelect={() => {
                                        if (deliverable.id) {
                                            navigate(`/deliverables/${deliverable.id}`);
                                            setOpen(false);
                                        }
                                    }}
                                >
                                    {deliverable.title}
                                </CommandItem>
                            ))}
                        </CommandGroup>
                    )}
                </CommandList>
            </CommandDialog>
        </div>
    );
}

export function Header() {
    return (
        <header className="flex h-16 items-center justify-between border-b px-4 md:px-6">
            <div className="flex items-center gap-4">
                <Logo />
            </div>
            <div className="flex items-center gap-4">
                <SearchBar />
            </div>
        </header>
    );
}
