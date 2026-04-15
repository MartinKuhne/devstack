import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useCreateProjectMutation } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    architecture: z.string().optional(),
    memory: z.string().optional(),
    githubUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface CreateProjectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
}

export function CreateProjectDialog({ open, onOpenChange, onSuccess }: CreateProjectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createProject, { loading }] = useCreateProjectMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<ProjectFormData>({
        resolver: zodResolver(projectSchema),
    });

    const onSubmit = async (data: ProjectFormData) => {
        setServerError(null);

        try {
            const result = await createProject({
                variables: {
                    input: {
                        name: data.name,
                        description: data.description ?? null,
                        architecture: data.architecture ?? null,
                        memory: data.memory ?? null,
                        githubUrl: data.githubUrl || null,
                    },
                },
            });

            const payload = result.data?.createProject;
            if (payload?.errors?.length) {
                setServerError(payload.errors.join(', '));
                return;
            }

            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to create project');
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
                    <DialogTitle>Create New Project</DialogTitle>
                    <DialogDescription>
                        Add a new project to track development work.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="name">Name *</Label>
                            <Input
                                id="name"
                                {...register('name')}
                                placeholder="My Project"
                            />
                            {errors.name && (
                                <p className="text-sm text-destructive">{errors.name.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                {...register('description')}
                                placeholder="Brief description of the project"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="architecture">Architecture</Label>
                            <Textarea
                                id="architecture"
                                {...register('architecture')}
                                placeholder="Technical architecture notes"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="memory">Memory</Label>
                            <Textarea
                                id="memory"
                                {...register('memory')}
                                placeholder="Project memory/context for AI agents"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="githubUrl">GitHub URL</Label>
                            <Input
                                id="githubUrl"
                                {...register('githubUrl')}
                                placeholder="https://github.com/user/repo"
                            />
                            {errors.githubUrl && (
                                <p className="text-sm text-destructive">{errors.githubUrl.message}</p>
                            )}
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
                            {loading ? 'Creating...' : 'Create Project'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
