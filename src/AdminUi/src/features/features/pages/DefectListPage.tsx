import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGetDefectsQuery } from '@/generated/graphql';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'react-toastify';
import { SeverityBadge } from '@/features/defects/components/SeverityBadge';
import { CreateDefectDialog } from '@/features/defects/components/CreateDefectDialog';

const STATUS_COLORS: Record<string, string> = {
    Reported: 'bg-gray-500',
    Triaged: 'bg-blue-500',
    InProgress: 'bg-yellow-500',
    Resolved: 'bg-purple-500',
    Closed: 'bg-green-500',
};

export function DefectListPage() {
    const navigate = useNavigate();
    const [statusFilter, setStatusFilter] = useState<string>('all');
    const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

    const { data, loading, error } = useGetDefectsQuery({
        fetchPolicy: 'cache-and-network',
    });
    
    const projectId = data?.defects.nodes?.[0]?.projectId ?? '2f20d4f1-cb84-4670-9117-1882f32d65cc';

    const handleRowClick = useCallback((defectId: string) => {
        navigate(`/defects/${defectId}`);
    }, [navigate]);

    const handleCreateDefect = useCallback((defectId: string) => {
        toast.success(`Defect ${defectId} created successfully`);
    }, [toast]);

    const filteredDefects = (data?.defects.nodes ?? []).filter(defect => {
        if (statusFilter === 'all') return true;
        return defect.status === statusFilter;
    });

    if (error) {
        toast.error('Failed to load defects');
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Defects</h2>
                    <p className="text-muted-foreground">Track and manage defects.</p>
                </div>
                <div className="flex items-center gap-2">
                    <Select value={statusFilter} onValueChange={setStatusFilter}>
                        <SelectTrigger className="w-[180px]">
                            <SelectValue placeholder="Filter by status" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Statuses</SelectItem>
                            <SelectItem value="Reported">Reported</SelectItem>
                            <SelectItem value="Triaged">Triaged</SelectItem>
                            <SelectItem value="InProgress">In Progress</SelectItem>
                            <SelectItem value="Resolved">Resolved</SelectItem>
                            <SelectItem value="Closed">Closed</SelectItem>
                        </SelectContent>
                    </Select>
                    <Button onClick={() => setIsCreateDialogOpen(true)}>
                        New Defect
                    </Button>
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Defect List</CardTitle>
                </CardHeader>
                <CardContent>
                    {loading ? (
                        <div className="space-y-4">
                            {[1, 2, 3].map((item) => (
                                <div
                                    key={item}
                                    className="flex items-center justify-between py-2 border-b last:border-0"
                                >
                                    <div className="space-y-1">
                                        <div className="h-4 w-64 bg-muted rounded animate-pulse" />
                                        <div className="h-3 w-48 bg-muted rounded animate-pulse" />
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <div className="h-6 w-24 bg-muted rounded animate-pulse" />
                                        <div className="h-6 w-20 bg-muted rounded animate-pulse" />
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : filteredDefects.length === 0 ? (
                        <p className="text-muted-foreground text-sm text-center py-8">
                            No defects found
                        </p>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Title</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Severity</TableHead>
                                    <TableHead>Updated</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {filteredDefects.map((defect) => (
                                    <TableRow
                                        key={defect.id ?? ''}
                                        className="cursor-pointer hover:bg-accent"
                                        onClick={() => handleRowClick(defect.id ?? '')}
                                    >
                                        <TableCell className="font-medium">
                                            {defect.title}
                                            {defect.description && (
                                                <p className="text-sm text-muted-foreground font-normal mt-1">
                                                    {defect.description.substring(0, 100)}
                                                    {defect.description.length > 100 && '...'}
                                                </p>
                                            )}
                                        </TableCell>
                                        <TableCell>
                                            <Badge className={STATUS_COLORS[defect.status ?? ''] || 'bg-gray-500'}>
                                                {defect.status}
                                            </Badge>
                                        </TableCell>
                                        <TableCell>
                                            {defect.severity && <SeverityBadge severity={defect.severity} />}
                                        </TableCell>
                                        <TableCell className="text-sm text-muted-foreground">
                                            {new Date(defect.updatedAt).toLocaleDateString()}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
            <CreateDefectDialog
                open={isCreateDialogOpen}
                onOpenChange={setIsCreateDialogOpen}
                projectId={projectId}
                onSuccess={handleCreateDefect}
            />
        </div>
    );
}
