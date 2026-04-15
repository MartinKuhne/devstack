import { useNavigate } from 'react-router-dom';
import { useState, useCallback, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useFeatures } from '@/features/features/hooks/useFeatures';
import { CreateFeatureDialog } from '@/features/features/components/CreateFeatureDialog';

const STATUS_COLORS: Record<string, string> = {
    Planned: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Review: 'bg-purple-500',
    Done: 'bg-green-500',
    Failed: 'bg-red-500',
};

export function FeatureListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    
    const statusFilter = searchParams.get('status') || undefined;
    const searchFilter = searchParams.get('search') || undefined;
    
    const { features, loading, error, refetch } = useFeatures(undefined, statusFilter ? [statusFilter as any] : undefined);
    const [localSearch, setLocalSearch] = useState(searchFilter || '');
    const [createDialogOpen, setCreateDialogOpen] = useState(false);

    useEffect(() => {
        if (searchFilter !== localSearch) {
            setLocalSearch(searchFilter || '');
        }
    }, [searchFilter]);

    const handleStatusChange = useCallback((value: string) => {
        const newParams = new URLSearchParams(searchParams);
        if (value === 'all') {
            newParams.delete('status');
        } else {
            newParams.set('status', value);
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
        if (id) navigate(`/features/${id}`);
    };

    const filteredFeatures = features.filter((feature) => {
        if (!searchFilter) return true;
        return (feature.title ?? '').toLowerCase().includes(searchFilter.toLowerCase());
    });

    if (error) {
        return (
            <div className="space-y-6">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Features</h2>
                    <p className="text-muted-foreground">Manage feature development.</p>
                </div>
                <Card>
                    <CardHeader>
                        <CardTitle className="text-destructive">Error loading features</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm text-destructive">{error.message}</p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Features</h2>
                    <p className="text-muted-foreground">Manage feature development.</p>
                </div>
                <Button onClick={() => setCreateDialogOpen(true)}>New Feature</Button>
            </div>

            <div className="flex gap-4 items-end">
                <div className="w-48">
                    <Select value={statusFilter || 'all'} onValueChange={handleStatusChange}>
                        <SelectTrigger>
                            <SelectValue placeholder="Filter by status" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Statuses</SelectItem>
                            <SelectItem value="Planned">Planned</SelectItem>
                            <SelectItem value="InProgress">In Progress</SelectItem>
                            <SelectItem value="Review">Review</SelectItem>
                            <SelectItem value="Done">Done</SelectItem>
                            <SelectItem value="Failed">Failed</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <form onSubmit={handleSearchSubmit} className="flex-1 max-w-sm">
                    <div className="flex gap-2">
                        <Input
                            placeholder="Search features..."
                            value={localSearch}
                            onChange={handleSearchChange}
                        />
                        <Button type="submit" variant="secondary">Search</Button>
                    </div>
                </form>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Feature List</CardTitle>
                </CardHeader>
                <CardContent>
                    {loading ? (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Tasks</TableHead>
                                    <TableHead>Updated</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {[1, 2, 3].map((item) => (
                                    <TableRow key={item}>
                                        <TableCell><Skeleton className="h-4 w-64" /></TableCell>
                                        <TableCell><Skeleton className="h-6 w-20" /></TableCell>
                                        <TableCell><Skeleton className="h-4 w-8" /></TableCell>
                                        <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    ) : filteredFeatures && filteredFeatures.length > 0 ? (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Tasks</TableHead>
                                    <TableHead>Updated</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {filteredFeatures.map((feature) => (
                                    <TableRow
                                        key={feature.id ?? ''}
                                        className="cursor-pointer hover:bg-muted/50"
                                        onClick={() => handleRowClick(feature.id ?? '')}
                                    >
                                        <TableCell className="font-medium">{feature.title}</TableCell>
                                        <TableCell>
                                            <Badge className={STATUS_COLORS[feature.status ?? ''] || 'bg-gray-500'}>
                                                {feature.status}
                                            </Badge>
                                        </TableCell>
                                        <TableCell>{feature.tasks?.length || 0}</TableCell>
                                        <TableCell>
                                            {feature.updatedAt ? new Date(feature.updatedAt).toLocaleDateString() : '-'}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    ) : (
                        <div className="py-8 text-center">
                            <p className="text-muted-foreground">No features found.</p>
                        </div>
                    )}
                </CardContent>
            </Card>
            <CreateFeatureDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                projectId=""
                onSuccess={(featureId) => {
                    refetch();
                    if (featureId) {
                        navigate(`/features/${featureId}`);
                    }
                }}
            />
        </div>
    );
}
