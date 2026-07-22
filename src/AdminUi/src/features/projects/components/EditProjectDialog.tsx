import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateProjectMutation } from '@/generated/graphql';
import { createModuleLogger, formatGraphQLError } from '@/lib/logging';

import { Button } from '@/components/ui/button';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const logger = createModuleLogger('EditProjectDialog');

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    repository: z.string().optional(),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface ProjectData {
    id: string;
    name: string;
    description: string | null;
    repository: string | null;
}

interface EditProjectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    project: ProjectData | null;
    onSuccess?: () => void;
}

export function EditProjectDialog({
    open,
    onOpenChange,
    project,
    onSuccess,
}: EditProjectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateProject, { loading }] = useUpdateProjectMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isValid },
    } = useForm<ProjectFormData>({
        resolver: zodResolver(projectSchema),
    });

    useEffect(() => {
        if (project && open) {
            reset({
                name: project.name,
                description: project.description ?? '',
                repository: project.repository ?? '',
            });
        }
    }, [project, open, reset]);

    const onSubmit = async (data: ProjectFormData) => {
        if (!project) return;

        setServerError(null);
        logger.info('Updating project', { id: project.id, name: data.name });

        try {
            const result = await updateProject({
                variables: {
                    input: {
                        id: project.id,
                        name: data.name,
                        description: data.description ?? null,
                        repository: data.repository ?? null,
                    },
                },
            });

            const resultProject = result.data?.updateProject;
            if (!resultProject) {
                logger.warn('Failed to update project', { id: project.id });
                setServerError('Failed to update project');
                return;
            }

            logger.info('Project updated successfully', { id: project.id, name: data.name });
            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            const errorInfo = formatGraphQLError(err);
            logger.error('Failed to update project', {
                id: project.id,
                name: data.name,
                error: errorInfo.message,
                details: errorInfo.details,
            });
            setServerError(err instanceof Error ? err.message : 'Failed to update project');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    if (!project) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Edit Project</DialogTitle>
                    <DialogDescription>
                        Update project details. Changes will be saved immediately.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="name">Name *</Label>
                            <Input id="name" {...register('name')} placeholder="My Project" />
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
                            <Label htmlFor="repository">Repository URL</Label>
                            <Input
                                id="repository"
                                {...register('repository')}
                                placeholder="https://github.com/user/repo"
                            />
                            {errors.repository && (
                                <p className="text-sm text-destructive">
                                    {errors.repository.message}
                                </p>
                            )}
                        </div>

                        {serverError && <p className="text-sm text-destructive">{serverError}</p>}
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => handleOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={!isValid || loading}>
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
