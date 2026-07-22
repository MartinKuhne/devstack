import { useNavigate } from 'react-router-dom';
import { useState, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
    PageHeader,
    FilterBar,
    DataPanel,
    LoadingState,
    ErrorState,
    EmptyState,
} from '@/components/layout';
import { Button } from '@/components/ui/button';
import { Trash2, ChevronUp, ChevronDown } from 'lucide-react';
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
import { useAllDeliverables } from '../hooks/useAllDeliverables';
import { useProjectContext } from '@/contexts/ProjectContext';
import { useDeleteDeliverable } from '../hooks/useDeleteDeliverable';
import { toast } from 'react-toastify';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';
import { DELIVERABLE_STATUS_COLORS, DELIVERABLE_STATUS_TEXT_COLORS, getStatusColor, getStatusTextColor, getStatusIcon } from '@/lib/constants';
import { ConfirmDialog } from '@/components/ConfirmDialog';
import { Pagination } from '@/components/ui/pagination';

const TYPE_LABELS: Record<string, string> = {
    FEATURE: 'Feature',
    DEFECT: 'Defect',
    MAINTENANCE: 'Maintenance',
    SPIKE: 'Spike',
};

const TYPE_FILTER_OPTIONS = [
    { value: 'all', label: 'All Types' },
    { value: 'FEATURE', label: 'Feature' },
    { value: 'DEFECT', label: 'Defect' },
    { value: 'MAINTENANCE', label: 'Maintenance' },
    { value: 'SPIKE', label: 'Spike' },
];

const STATUS_FILTER_OPTIONS = [
    { value: 'all', label: 'All Statuses' },
    { value: 'DRAFT', label: 'Draft' },
    { value: 'DESIGN', label: 'Design' },
    { value: 'PLAN', label: 'Plan' },
    { value: 'IMPLEMENT', label: 'Implement' },
    { value: 'MERGE', label: 'Merge' },
    { value: 'DEPLOY', label: 'Deploy' },
    { value: 'TEST', label: 'Test' },
    { value: 'DONE', label: 'Done' },
    { value: 'NEEDS_REVIEW', label: 'Needs Review' },
    { value: 'FAILED', label: 'Failed' },
    { value: 'REJECTED', label: 'Rejected' },
];

export function DeliverableListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const { projectId } = useProjectContext();

    const statusFilter = (searchParams.get('status') || undefined) as DeliverableStatus | undefined;
    const typeFilter = (searchParams.get('type') || undefined) as DeliverableType | undefined;
    const searchFilter = searchParams.get('search') || undefined;

    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [localSearch, setLocalSearch] = useState(searchFilter || '');
    const { deleteDeliverable, loading: deleteLoading } = useDeleteDeliverable();
    const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [sortField, setSortField] = useState<'title' | 'status' | 'type'>('title');
    const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
    const pageSize = 25;

    const { deliverables, loading, error, refetch } = useAllDeliverables(
        statusFilter ? [statusFilter] : undefined,
        typeFilter ? [typeFilter] : undefined
    );

    const handleDelete = async () => {
        if (!deleteTargetId) return;

        const result = await deleteDeliverable(deleteTargetId);

        if (result.success) {
            toast.success('Deliverable deleted successfully');
            refetch();
        } else {
            toast.error(result.errors?.join(', ') ?? 'Failed to delete deliverable');
        }
        setDeleteTargetId(null);
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

    const handleRowKeyDown = (e: React.KeyboardEvent, id: string | null | undefined) => {
        if ((e.key === 'Enter' || e.key === ' ') && id) {
            e.preventDefault();
            navigate(`/deliverables/${id}`);
        }
    };

    const filteredDeliverables = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && (
        !searchFilter ||
        !!d.title?.toLowerCase().includes(searchFilter.toLowerCase())
    ));

    const sortedDeliverables = useMemo(() => {
        return [...filteredDeliverables].sort((a, b) => {
            const aVal = (a[sortField] ?? '').toString().toLowerCase();
            const bVal = (b[sortField] ?? '').toString().toLowerCase();
            if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
            if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
            return 0;
        });
    }, [filteredDeliverables, sortField, sortDirection]);

    const totalPages = Math.ceil(sortedDeliverables.length / pageSize);
    const paginatedDeliverables = sortedDeliverables.slice(
        (currentPage - 1) * pageSize,
        currentPage * pageSize
    );

    const handleSort = (field: 'title' | 'status' | 'type') => {
        if (sortField === field) {
            setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDirection('asc');
        }
        setCurrentPage(1);
    };

    const renderSortIcon = (field: 'title' | 'status' | 'type') => {
        if (sortField !== field) return null;
        return sortDirection === 'asc' ? <ChevronUp className="h-3 w-3 ml-1" /> : <ChevronDown className="h-3 w-3 ml-1" />;
    };

    const renderStatusIcon = (status: string | undefined) => {
        const Icon = getStatusIcon(status, 'deliverable');
        return Icon ? <Icon className="mr-1 h-3 w-3" /> : null;
    };

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
                    <>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('title')}>
                                        Title {renderSortIcon('title')}
                                    </button>
                                </TableHead>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('type')}>
                                        Type {renderSortIcon('type')}
                                    </button>
                                </TableHead>
                                <TableHead>
                                    <button type="button" className="flex items-center font-medium" onClick={() => handleSort('status')}>
                                        Status {renderSortIcon('status')}
                                    </button>
                                </TableHead>
                                <TableHead>ID</TableHead>
                                <TableHead className="w-16"></TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {paginatedDeliverables.map((deliverable) => (
                                <TableRow
                                    key={deliverable.id ?? ''}
                                    className="cursor-pointer hover:bg-muted/50"
                                    tabIndex={0}
                                    role="button"
                                    onClick={() => handleRowClick(deliverable.id ?? undefined)}
                                    onKeyDown={(e) => handleRowKeyDown(e, deliverable.id ?? undefined)}
                                >
                                    <TableCell className="font-medium">
                                        {deliverable.title}
                                    </TableCell>
                                    <TableCell>
                                        {TYPE_LABELS[deliverable.type ?? ''] ?? deliverable.type}
                                    </TableCell>
                                    <TableCell>
                                        <Badge
                                            className={`${getStatusColor(
                                                deliverable.status ?? undefined,
                                                DELIVERABLE_STATUS_COLORS
                                            )} ${getStatusTextColor(
                                                deliverable.status ?? undefined,
                                                DELIVERABLE_STATUS_TEXT_COLORS
                                            )}`}
                                        >
                                            {renderStatusIcon(deliverable.status ?? undefined)}
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
                                                deliverable.id && setDeleteTargetId(deliverable.id)
                                            }
                                            disabled={deleteLoading}
                                            aria-label={`Delete deliverable ${deliverable.title}`}
                                        >
                                            <Trash2 className="h-4 w-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                    <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
                    </>
                ) : (
                    <EmptyState
                        description="No deliverables found."
                        action={{ label: 'New Deliverable', onClick: () => setCreateDialogOpen(true) }}
                    />
                )}
            </DataPanel>

            <CreateDeliverableDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                projectId={projectId}
                onSuccess={() => {
                    toast.success('Deliverable created successfully');
                    refetch();
                }}
            />
            <ConfirmDialog
                open={!!deleteTargetId}
                onOpenChange={(open) => !open && setDeleteTargetId(null)}
                title="Delete Deliverable"
                description="Are you sure you want to delete this deliverable? This action cannot be undone."
                confirmLabel="Delete"
                variant="destructive"
                onConfirm={handleDelete}
            />
        </div>
    );
}
