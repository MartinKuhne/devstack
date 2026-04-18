import { useNavigate } from 'react-router-dom';
import { useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { CreateDeliverableDialog } from '../components/CreateDeliverableDialog';

export function DeliverableListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    
    const statusFilter = searchParams.get('status') || undefined;
    const typeFilter = searchParams.get('type') || undefined;
    const searchFilter = searchParams.get('search') || undefined;
    
    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [localSearch, setLocalSearch] = useState(searchFilter || '');

    const handleStatusChange = useCallback((value: string) => {
        const newParams = new URLSearchParams(searchParams);
        if (value === 'all') {
            newParams.delete('status');
        } else {
            newParams.set('status', value);
        }
        setSearchParams(newParams);
    }, [searchParams, setSearchParams]);

    const handleTypeChange = useCallback((value: string) => {
        const newParams = new URLSearchParams(searchParams);
        if (value === 'all') {
            newParams.delete('type');
        } else {
            newParams.set('type', value);
        }
        setSearchParams(newParams);
    }, [searchParams, setSearchParams]);

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setLocalSearch(e.target.value);
    }, []);

    const handleSearchSubmit = useCallback((e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const newParams = new URLSearchParams(searchParams);
        if (localSearch) {
            newParams.set('search', localSearch);
        } else {
            newParams.delete('search');
        }
        setSearchParams(newParams);
    }, [localSearch, searchParams, setSearchParams]);

    const handleRowClick = (id: string | null | undefined) => {
        if (id) navigate(`/deliverables/${id}`);
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Deliverables</h2>
                    <p className="text-muted-foreground">Manage deliverables (features, defects, maintenance).</p>
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
                        <Button type="submit" variant="secondary">Search</Button>
                    </div>
                </form>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Deliverable List</CardTitle>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Title</TableHead>
                                <TableHead>Type</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead>Updated</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow className="cursor-pointer hover:bg-muted/50" onClick={() => handleRowClick(null)}>
                                <TableCell className="font-medium">No deliverables yet</TableCell>
                                <TableCell>-</TableCell>
                                <TableCell>-</TableCell>
                                <TableCell>-</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>
            
            <CreateDeliverableDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                onSuccess={(deliverableId) => {
                    if (deliverableId) {
                        navigate(`/deliverables/${deliverableId}`);
                    }
                }}
            />
        </div>
    );
}
