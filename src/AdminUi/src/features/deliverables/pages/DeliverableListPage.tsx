import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Trash2 } from 'lucide-react';
import { Input } from '@/components/ui/input';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
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

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setLocalSearch(e.target.value);
    }, []);

    const handleSearchSubmit = useCallback(
        (e: React.FormEvent<HTMLFormElement>) => {
            e.preventDefault();
            const newParams = new URLSearchParams(searchParams);
            if (localSearch) {
                newParams.set('search', localSearch);
            } else {
                newParams.delete('search');
            }
            setSearchParams(newParams);
        },
        [localSearch, searchParams, setSearchParams]
    );

    const handleRowClick = (id: string | null | undefined) => {
        if (id) navigate(`/deliverables/${id}`);
    };

    const filteredDeliverables = deliverables.filter((d) => {
        if (!searchFilter) return true;
        const searchLower = searchFilter.toLowerCase();
        return d.title?.toLowerCase().includes(searchLower) ?? false;
    });

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Deliverables</h2>
                    <p className="text-muted-foreground">
                        Manage deliverables (features, defects, maintenance).
                    </p>
                </div>
                <Button onClick={() => setCreateDialogOpen(true)}>New Deliverable</Button>
            </div>

            <div className="flex gap-4 items-end">
                <div className="w-48">
                    <Select value={typeFilter || 'all'} onValueChange={handleTypeChange}>
                        <SelectTrigger>
                            <SelectValue placeholder="Filter by type" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Types</SelectItem>
                            <SelectItem value="FEATURE">Feature</SelectItem>
                            <SelectItem value="DEFECT">Defect</SelectItem>
                            <SelectItem value="MAINTENANCE">Maintenance</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <div className="w-48">
                    <Select value={statusFilter || 'all'} onValueChange={handleStatusChange}>
                        <SelectTrigger>
                            <SelectValue placeholder="Filter by status" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Statuses</SelectItem>
                            <SelectItem value="DRAFT">Draft</SelectItem>
                            <SelectItem value="PLANNING">Planning</SelectItem>
                            <SelectItem value="READY">Ready</SelectItem>
                            <SelectItem value="IN_PROGRESS">In Progress</SelectItem>
                            <SelectItem value="NEEDS_REVIEW">Needs Review</SelectItem>
                            <SelectItem value="DONE">Done</SelectItem>
                            <SelectItem value="FAILED">Failed</SelectItem>
                            <SelectItem value="REJECTED">Rejected</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <form onSubmit={handleSearchSubmit} className="flex-1 max-w-sm">
                    <div className="flex gap-2">
                        <Input
                            placeholder="Search deliverables..."
                            value={localSearch}
                            onChange={handleSearchChange}
                        />
                        <Button type="submit" variant="secondary">
                            Search
                        </Button>
                    </div>
                </form>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Deliverable List</CardTitle>
                </CardHeader>
                <CardContent>
                    {error ? (
                        <div className="space-y-4">
                            <p className="text-sm text-destructive">{error.message}</p>
                            <p className="text-sm text-muted-foreground">
                                This error has been logged. Please try refreshing the page or
                                contact support.
                            </p>
                            <Button variant="outline" onClick={() => refetch()}>
                                Retry
                            </Button>
                        </div>
                    ) : loading ? (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Type</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>ID</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {[1, 2, 3].map((item) => (
                                    <TableRow key={item}>
                                        <TableCell>
                                            <Skeleton className="h-4 w-64" />
                                        </TableCell>
                                        <TableCell>
                                            <Skeleton className="h-4 w-20" />
                                        </TableCell>
                                        <TableCell>
                                            <Skeleton className="h-6 w-20" />
                                        </TableCell>
                                        <TableCell>
                                            <Skeleton className="h-4 w-24" />
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
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
                                            {TYPE_LABELS[deliverable.type ?? ''] ??
                                                deliverable.type}
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
                </CardContent>
            </Card>

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
