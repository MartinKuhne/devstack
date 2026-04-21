import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
    PageHeader,
    FilterBar,
    DataPanel,
    LoadingState,
    ErrorState,
} from '@/components/layout';
import { Button } from '@/components/ui/button';
import { Trash2 } from 'lucide-react';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { CreateDeliverableDialog } from '../components/CreateDeliverableDialog';
import { useDeliverables } from '../hooks/useDeliverables';
import { toast } from 'react-toastify';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';
import { DELIVERABLE_STATUS_COLORS, getStatusColor } from '@/lib/constants';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('DeliverableListPage');

const TYPE_LABELS: Record<string, string> = {
    FEATURE: 'Feature',
    DEFECT: 'Defect',
    MAINTENANCE: 'Maintenance',
};

const TYPE_FILTER_OPTIONS = [
    { value: 'all', label: 'All Types' },
    { value: 'FEATURE', label: 'Feature' },
    { value: 'DEFECT', label: 'Defect' },
    { value: 'MAINTENANCE', label: 'Maintenance' },
];

const STATUS_FILTER_OPTIONS = [
    { value: 'all', label: 'All Statuses' },
    { value: 'DRAFT', label: 'Draft' },
    { value: 'PLANNING', label: 'Planning' },
    { value: 'READY', label: 'Ready' },
    { value: 'IN_PROGRESS', label: 'In Progress' },
    { value: 'NEEDS_REVIEW', label: 'Needs Review' },
    { value: 'DONE', label: 'Done' },
    { value: 'FAILED', label: 'Failed' },
    { value: 'REJECTED', label: 'Rejected' },
];

export function DeliverableListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();

    const statusFilter = (searchParams.get('status') || undefined) as DeliverableStatus | undefined;
    const typeFilter = (searchParams.get('type') || undefined) as DeliverableType | undefined;
    const searchFilter = searchParams.get('search') || undefined;

    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [localSearch, setLocalSearch] = useState(searchFilter || '');
    const [deleting, setDeleting] = useState(false);

    const { deliverables, loading, error, refetch } = useDeliverables(
        statusFilter ? [statusFilter] : undefined,
        typeFilter ? [typeFilter] : undefined
    );

    const handleDelete = async (id: string) => {
        if (!confirm('Are you sure you want to delete this deliverable?')) return;
        setDeleting(true);
        logger.info('Deleting deliverable', { id });
        try {
            const response = await fetch('/graphql', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query: 'mutation DeleteDeliverable($input: DeleteDeliverableInput!) { deleteDeliverable(input: $input) { deliverable { id } errors } }',
                    variables: { input: { id } },
                }),
            });
            const result = await response.json();
            if (result?.data?.deleteDeliverable?.errors?.length) {
                const errorMessage = result.data.deleteDeliverable.errors.join(', ');
                logger.warn('Failed to delete deliverable', {
                    id,
                    errors: result.data.deleteDeliverable.errors,
                });
                toast.error(errorMessage);
            } else {
                logger.info('Deliverable deleted successfully', { id });
                toast.success('Deliverable deleted successfully');
                refetch();
            }
        } catch (err) {
            logger.error('Failed to delete deliverable', {
                id,
                error: err instanceof Error ? err.message : String(err),
            });
            toast.error(err instanceof Error ? err.message : 'Failed to delete deliverable');
        } finally {
            setDeleting(false);
        }
    };

    const handleStatusChange = useCallback(
        (value: string) => {
            const newParams = new URLSearchParams(searchParams);
            if (value === 'all') {
                newParams.delete('status');
            } else {
                newParams.set('status', value);
            }
            setSearchParams(newParams);
        },
        [searchParams, setSearchParams]
    );

    const handleTypeChange = useCallback(
        (value: string) => {
            const newParams = new URLSearchParams(searchParams);
            if (value === 'all') {
                newParams.delete('type');
            } else {
                newParams.set('type', value);
            }
            setSearchParams(newParams);
        },
        [searchParams, setSearchParams]
    );

    const handleSearchChange = useCallback((value: string) => {
        setLocalSearch(value);
    }, []);

    const handleSearchSubmit = useCallback(() => {
        const newParams = new URLSearchParams(searchParams);
        if (localSearch) {
            newParams.set('search', localSearch);
        } else {
            newParams.delete('search');
        }
        setSearchParams(newParams);
    }, [localSearch, searchParams, setSearchParams]);

    const handleSearchClear = useCallback(() => {
        setLocalSearch('');
    }, []);

    const handleRowClick = (id: string | null | undefined) => {
        if (id) navigate(`/deliverables/${id}`);
    };

    const filteredDeliverables = deliverables.filter((d) => {
        if (!searchFilter) return true;
        const searchLower = searchFilter.toLowerCase();
        return d.title?.toLowerCase().includes(searchLower) ?? false;
    });

    const handleCreateDeliverable = useCallback(() => {
        setCreateDialogOpen(true);
    }, []);

    const handleRetry = useCallback(() => {
        refetch();
    }, [refetch]);

    return (
        <div className="space-y-6">
            <PageHeader
                title="Deliverables"
                description="Manage deliverables (features, defects, maintenance)."
                actionSlot={<Button onClick={handleCreateDeliverable}>New Deliverable</Button>}
            />

            <FilterBar
                searchValue={localSearch}
                onSearchChange={handleSearchChange}
                onSearchSubmit={handleSearchSubmit}
                onSearchClear={handleSearchClear}
                selects={[
                    {
                        value: typeFilter || 'all',
                        onChange: handleTypeChange,
                        options: TYPE_FILTER_OPTIONS,
                    },
                    {
                        value: statusFilter || 'all',
                        onChange: handleStatusChange,
                        options: STATUS_FILTER_OPTIONS,
                    },
                ]}
            />

            <DataPanel title="Deliverable List">
                {error ? (
                    <ErrorState
                        message={error.message}
                        detail="This error has been logged. Please try refreshing the page or contact support."
                        onRetry={handleRetry}
                    />
                ) : loading ? (
                    <LoadingState rows={3} />
                ) : filteredDeliverables.length > 0 ? (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Title</TableHead>
                                <TableHead>Type</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead>ID</TableHead>
                                <TableHead className="w-16"></TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {filteredDeliverables.map((deliverable) => (
                                <TableRow
                                    key={deliverable.id ?? ''}
                                    className="cursor-pointer hover:bg-muted/50"
                                    onClick={() => handleRowClick(deliverable.id ?? undefined)}
                                >
                                    <TableCell className="font-medium">
                                        {deliverable.title}
                                    </TableCell>
                                    <TableCell>
                                        {TYPE_LABELS[deliverable.type ?? ''] ?? deliverable.type}
                                    </TableCell>
                                    <TableCell>
                                        <Badge
                                            className={getStatusColor(
                                                deliverable.status ?? undefined,
                                                DELIVERABLE_STATUS_COLORS
                                            )}
                                        >
                                            {deliverable.status}
                                        </Badge>
                                    </TableCell>
                                    <TableCell className="text-xs font-mono">
                                        {deliverable.id ?? '-'}
                                    </TableCell>
                                    <TableCell onClick={(e) => e.stopPropagation()}>
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-8 w-8 text-destructive hover:text-destructive"
                                            onClick={() =>
                                                deliverable.id && handleDelete(deliverable.id)
                                            }
                                            disabled={deleting}
                                        >
                                            <Trash2 className="h-4 w-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                ) : (
                    <p className="text-muted-foreground text-sm">No deliverables found.</p>
                )}
            </DataPanel>

            <CreateDeliverableDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                onSuccess={(deliverableId) => {
                    refetch();
                    if (deliverableId) {
                        navigate(`/deliverables/${deliverableId}`);
                    }
                }}
            />
        </div>
    );
}
