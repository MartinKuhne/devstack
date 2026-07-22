import type { LucideIcon } from 'lucide-react';
import { Check, AlertCircle, Eye, Loader, Circle, CircleDot, XCircle, Play } from 'lucide-react';

export type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'success' | 'warning';

export const DELIVERABLE_STATUS_VARIANTS: Record<string, BadgeVariant> = {
    DRAFT: 'secondary',
    DESIGN: 'default',
    PLAN: 'default',
    IMPLEMENT: 'warning',
    MERGE: 'default',
    DEPLOY: 'success',
    TEST: 'warning',
    DONE: 'success',
    FAILED: 'destructive',
    REJECTED: 'destructive',
    NEEDS_REVIEW: 'warning',
};

export const PROJECT_STATUS_VARIANTS: Record<string, BadgeVariant> = {
    PLANNING: 'default',
    READY: 'success',
    IN_PROGRESS: 'warning',
    NEEDS_REVIEW: 'warning',
    DONE: 'success',
    FAILED: 'destructive',
    REJECTED: 'destructive',
};

export const AGENT_TASK_STATUS_VARIANTS: Record<string, BadgeVariant> = {
    READY: 'default',
    IN_PROGRESS: 'warning',
    NEEDS_REVIEW: 'warning',
    DONE: 'success',
    FAILED: 'destructive',
    REJECTED: 'destructive',
};

const DELIVERABLE_STATUS_ICONS: Record<string, LucideIcon> = {
    DRAFT: CircleDot,
    DESIGN: Circle,
    PLAN: Circle,
    IMPLEMENT: Play,
    MERGE: Play,
    DEPLOY: Play,
    TEST: Eye,
    DONE: Check,
    FAILED: AlertCircle,
    REJECTED: XCircle,
    NEEDS_REVIEW: Eye,
};

const PROJECT_STATUS_ICONS: Record<string, LucideIcon> = {
    PLANNING: Circle,
    READY: Check,
    IN_PROGRESS: Loader,
    NEEDS_REVIEW: Eye,
    DONE: Check,
    FAILED: AlertCircle,
    REJECTED: XCircle,
};

const AGENT_TASK_STATUS_ICONS: Record<string, LucideIcon> = {
    READY: Circle,
    IN_PROGRESS: Loader,
    NEEDS_REVIEW: Eye,
    DONE: Check,
    FAILED: AlertCircle,
    REJECTED: XCircle,
};

export function getStatusIcon(status: string | undefined, entity: 'deliverable' | 'project' | 'agentTask'): LucideIcon | undefined {
    if (!status) return undefined;
    switch (entity) {
        case 'deliverable':
            return DELIVERABLE_STATUS_ICONS[status];
        case 'project':
            return PROJECT_STATUS_ICONS[status];
        case 'agentTask':
            return AGENT_TASK_STATUS_ICONS[status];
    }
}

export function getStatusVariant(status: string | undefined, variantMap: Record<string, BadgeVariant>): BadgeVariant {
    if (!status) return 'secondary';
    return variantMap[status] ?? 'secondary';
}

const VARIANT_TO_BG: Record<BadgeVariant, string> = {
    default: 'bg-primary',
    secondary: 'bg-muted',
    destructive: 'bg-destructive',
    success: 'bg-success',
    warning: 'bg-warning',
};

const VARIANT_TO_TEXT: Record<BadgeVariant, string> = {
    default: 'text-primary-foreground',
    secondary: 'text-foreground',
    destructive: 'text-destructive-foreground',
    success: 'text-success-foreground',
    warning: 'text-warning-foreground',
};

export const DELIVERABLE_STATUS_COLORS = Object.fromEntries(
    Object.entries(DELIVERABLE_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

export const DELIVERABLE_STATUS_TEXT_COLORS = Object.fromEntries(
    Object.entries(DELIVERABLE_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_TEXT[v]])
) as Record<string, string>;

export const PROJECT_STATUS_COLORS = Object.fromEntries(
    Object.entries(PROJECT_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

export const PROJECT_STATUS_TEXT_COLORS = Object.fromEntries(
    Object.entries(PROJECT_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_TEXT[v]])
) as Record<string, string>;

export const AGENT_TASK_STATUS_COLORS = Object.fromEntries(
    Object.entries(AGENT_TASK_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

export const AGENT_TASK_STATUS_TEXT_COLORS = Object.fromEntries(
    Object.entries(AGENT_TASK_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_TEXT[v]])
) as Record<string, string>;

const DEFAULT_COLOR = 'bg-muted';
const DEFAULT_TEXT_COLOR = 'text-foreground';

export function getStatusColor(status: string | undefined, colorMap: Record<string, string>): string {
    if (!status) return DEFAULT_COLOR;
    return colorMap[status] || DEFAULT_COLOR;
}

export function getStatusTextColor(status: string | undefined, colorMap: Record<string, string>): string {
    if (!status) return DEFAULT_TEXT_COLOR;
    return colorMap[status] || DEFAULT_TEXT_COLOR;
}
