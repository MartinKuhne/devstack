import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateEpicMutation } from '@/generated/graphql';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const epicSchema = z.object({
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
});

type EpicFormData = z.infer<typeof epicSchema>;

interface EditEpicDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    epic: { id: string; title: string; description: string | null };
    onSuccess?: () => void;
}

export function EditEpicDialog({ open, onOpenChange, epic, onSuccess }: EditEpicDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateEpic, { loading }] = useUpdateEpicMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<EpicFormData>({
        resolver: zodResolver(epicSchema),
        defaultValues: {
            title: '',
            description: '',
        },
    });

    useEffect(() => {
        if (open && epic) {
            reset({
                title: epic.title,
                description: epic.description ?? '',
            });
        }
    }, [open, epic, reset]);

    const onSubmit = async (data: EpicFormData) => {
        setServerError(null);

        try {
            const result = await updateEpic({
                variables: {
                    input: {
                        id: epic.id,
                        title: data.title,
                        description: data.description ?? null,
                    },
                },
            });

            const payload = result.data?.updateEpic;
            if (payload?.errors?.length) {
                setServerError(payload.errors.join(', '));
                return;
            }

            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to update epic');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Edit Epic</DialogTitle>
                    <DialogDescription>
                        Update the epic details.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Epic title"
                            />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                {...register('description')}
                                placeholder="Describe the epic"
                                rows={3}
                            />
                        </div>

                        {serverError && (
                            <p className="text-sm text-destructive">{serverError}</p>
                        )}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => handleOpenChange(false)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={loading}>
                            {loading ? 'Updating...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}