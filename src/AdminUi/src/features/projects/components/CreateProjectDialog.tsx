import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useCreateProjectMutation } from '@/generated/graphql';
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

const logger = createModuleLogger('CreateProjectDialog');

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    repository: z.string().optional(),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface CreateProjectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
}

/**
 * Dialog form for creating a new project with validation.
 */
export function CreateProjectDialog({ open, onOpenChange, onSuccess }: CreateProjectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createProject, { loading }] = useCreateProjectMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isValid },
    } = useForm<ProjectFormData>({
        resolver: zodResolver(projectSchema),
    });

    const onSubmit = async (data: ProjectFormData) => {
        setServerError(null);
        logger.info('Creating project', { name: data.name });

        try {
            const result = await createProject({
                variables: {
                    input: {
                        name: data.name,
                        description: data.description ?? null,
                        repository: data.repository ?? null,
                    },
                },
            });

            const payload = result.data?.createProject;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                logger.warn('Failed to create project: validation error', {
                    name: data.name,
                    errors: payload.errors,
                });
                setServerError(errorMessage);
                return;
            }

            logger.info('Project created successfully', { name: data.name });
            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            const errorInfo = formatGraphQLError(err);
            logger.error('Failed to create project', {
                name: data.name,
                error: errorInfo.message,
                details: errorInfo.details,
            });
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
                            {loading ? 'Creating...' : 'Create Project'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
