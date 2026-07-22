import { ChevronRight, Home } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';

export type BreadcrumbItem = {
    label: string;
    to?: string;
};

export type DetailLayoutProps = {
    breadcrumbs: BreadcrumbItem[];
    title: string;
    typeLabel?: string;
    statusNode?: ReactNode;
    actions?: ReactNode;
    children?: ReactNode;
    className?: string;
};

export function DetailLayout({
    breadcrumbs,
    title,
    typeLabel,
    statusNode,
    actions,
    children,
    className,
}: DetailLayoutProps) {
    return (
        <div className={className ?? 'space-y-6'}>
            <nav aria-label="Breadcrumb">
                <ol className="flex items-center gap-1.5 text-sm text-muted-foreground">
                    <li>
                        <Link to="/" className="hover:text-foreground transition-colors">
                            <Home className="h-4 w-4" />
                            <span className="sr-only">Home</span>
                        </Link>
                    </li>
                    {breadcrumbs.map((crumb, index) => (
                        <li key={index} className="flex items-center gap-1.5">
                            <ChevronRight className="h-3.5 w-3.5" />
                            {crumb.to && index < breadcrumbs.length - 1 ? (
                                <Link
                                    to={crumb.to}
                                    className="hover:text-foreground transition-colors"
                                >
                                    {crumb.label}
                                </Link>
                            ) : (
                                <span className="text-foreground font-medium">{crumb.label}</span>
                            )}
                        </li>
                    ))}
                </ol>
            </nav>

            <div className="flex items-start justify-between gap-4 border-b pb-4">
                <div className="flex items-center gap-3 flex-wrap">
                    <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
                    {typeLabel && (
                        <span className="text-sm text-muted-foreground uppercase">{typeLabel}</span>
                    )}
                    {statusNode}
                </div>
                {actions && <div className="flex items-center gap-2">{actions}</div>}
            </div>

            {children}
        </div>
    );
}
