import { useState } from 'react';
import { useGetEpicsQuery, useDeleteEpicMutation } from '@/generated/graphql';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { CreateEpicDialog } from '../components/CreateEpicDialog';
import { EditEpicDialog } from '../components/EditEpicDialog';
import { Trash2, Pencil, Plus } from 'lucide-react';

export function EpicListPage() {
    const [searchTitle, setSearchTitle] = useState('');
    const [createDialogOpen, setCreateDialogOpen] = useState(false);
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [selectedEpic, setSelectedEpic] = useState<{ id: string; title: string; description: string | null } | undefined>(undefined);

    const { data, loading, error, refetch } = useGetEpicsQuery({
        variables: { title: searchTitle || null },
        fetchPolicy: 'cache-and-network',
    });

    const [deleteEpic, { loading: deleting }] = useDeleteEpicMutation();

    const handleDelete = async (id: string) => {
        if (!confirm('Are you sure you want to delete this epic?')) {
            return;
        }
        try {
            await deleteEpic({ variables: { input: { id } } });
            refetch();
        } catch (err) {
            alert(err instanceof Error ? err.message : 'Failed to delete epic');
        }
    };

    const handleEdit = (epic: { id: string; title: string; description: string | null | undefined }) => {
        setSelectedEpic({
            id: epic.id,
            title: epic.title,
            description: epic.description ?? null,
        });
        setEditDialogOpen(true);
    };

    if (error) {
        return (
            <Card>
                <CardHeader>
                    <CardTitle className="text-destructive">Error loading epics</CardTitle>
                </CardHeader>
                <CardContent>
                    <p className="text-sm text-destructive">{error.message}</p>
                </CardContent>
            </Card>
        );
    }

    const epics = data?.epics?.nodes ?? [];
    const totalCount = data?.epics?.nodes?.length ?? 0;

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <h2 className="text-2xl font-bold tracking-tight">Epics</h2>
                <Button onClick={() => setCreateDialogOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    Create Epic
                </Button>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Filters</CardTitle>
                </CardHeader>
                <CardContent>
                    <Input
                        placeholder="Search by title..."
                        value={searchTitle}
                        onChange={(e) => setSearchTitle(e.target.value)}
                        className="max-w-sm"
                    />
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Epics ({totalCount})</CardTitle>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Title</TableHead>
                                <TableHead>Description</TableHead>
                                <TableHead>Created</TableHead>
                                <TableHead>Updated</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="text-center">
                                        Loading...
                                    </TableCell>
                                </TableRow>
                            ) : epics.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="text-center">
                                        No epics found
                                    </TableCell>
                                </TableRow>
                            ) : (
                                epics.map((epic) => (
                                    <TableRow key={epic.id ?? ''}>
                                        <TableCell className="font-medium">{epic.title}</TableCell>
                                        <TableCell className="max-w-xs truncate">
                                            {epic.description}
                                        </TableCell>
                                        <TableCell>
                                            {epic.createdAt
                                                ? new Date(epic.createdAt).toLocaleDateString()
                                                : '-'}
                                        </TableCell>
                                        <TableCell>
                                            {epic.updatedAt
                                                ? new Date(epic.updatedAt).toLocaleDateString()
                                                : '-'}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() =>
                                                        handleEdit({
                                                            id: epic.id ?? '',
                                                            title: epic.title ?? '',
                                                            description: epic.description ?? null,
                                                        })
                                                    }
                                                >
                                                    <Pencil className="h-4 w-4" />
                                                </Button>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() => handleDelete(epic.id ?? '')}
                                                    disabled={deleting}
                                                >
                                                    <Trash2 className="h-4 w-4" />
                                                </Button>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>

            <CreateEpicDialog
                open={createDialogOpen}
                onOpenChange={setCreateDialogOpen}
                onSuccess={() => {
                    refetch();
                    setCreateDialogOpen(false);
                }}
            />

            {selectedEpic !== undefined && (
                <EditEpicDialog
                    open={editDialogOpen}
                    onOpenChange={setEditDialogOpen}
                    epic={selectedEpic}
                    onSuccess={() => {
                        refetch();
                        setEditDialogOpen(false);
                        setSelectedEpic(undefined);
                    }}
                />
            )}
        </div>
    );
}