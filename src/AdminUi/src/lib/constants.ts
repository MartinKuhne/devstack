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

export const DELIVERABLE_STATUS_COLORS = Object.fromEntries(
    Object.entries(DELIVERABLE_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

export const PROJECT_STATUS_COLORS = Object.fromEntries(
    Object.entries(PROJECT_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

export const AGENT_TASK_STATUS_COLORS = Object.fromEntries(
    Object.entries(AGENT_TASK_STATUS_VARIANTS).map(([k, v]) => [k, VARIANT_TO_BG[v]])
) as Record<string, string>;

const DEFAULT_COLOR = 'bg-muted';

export function getStatusColor(status: string | undefined, colorMap: Record<string, string>): string {
    if (!status) return DEFAULT_COLOR;
    return colorMap[status] || DEFAULT_COLOR;
}
