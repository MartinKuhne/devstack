import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { gql } from '@apollo/client/core';
import { useMutation } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { UpdateProjectMutation, UpdateProjectMutationVariables } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const UPDATE_PROJECT = gql`
    mutation UpdateProject($id: ID!, $input: UpdateProjectInput!) {
        updateProject(id: $id, input: $input) {
            id
            name
            description
            architecture
            memory
            githubUrl
        }
    }
`;

const projectSchema = z.object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or less'),
    description: z.string().optional(),
    architecture: z.string().optional(),
    memory: z.string().optional(),
    githubUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
});

type ProjectFormData = z.infer<typeof projectSchema>;

interface ProjectData {
    id: string;
    name: string;
    description: string | null;
    architecture: string | null;
    memory: string | null;
    githubUrl: string | null;
}

interface EditProjectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    project: ProjectData | null;
    onSuccess?: () => void;
    onError?: (error: string) => void;
}

export function EditProjectDialog({ open, onOpenChange, project, onSuccess, onError }: EditProjectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateProject, { loading }] = useMutation<UpdateProjectMutation, UpdateProjectMutationVariables>(UPDATE_PROJECT, {
        client: getApolloClient(),
    });

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<ProjectFormData>({
        resolver: zodResolver(projectSchema),
    });

    useEffect(() => {
        if (project && open) {
            reset({
                name: project.name,
                description: project.description ?? '',
                architecture: project.architecture ?? '',
                memory: project.memory ?? '',
                githubUrl: project.githubUrl ?? '',
            });
        }
    }, [project, open, reset]);

    const onSubmit = async (data: ProjectFormData) => {
        if (!project) return;
        
        setServerError(null);
        
        try {
                await updateProject({
                    variables: {
                        id: project.id,
                        input: {
                            name: data.name,
                            description: data.description ?? null,
                            architecture: data.architecture ?? null,
                            memory: data.memory ?? null,
                            githubUrl: data.githubUrl || null,
                            version: 1,
                        },
                    },
                });

            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to update project';
            
            if (errorMessage.includes('NOT_FOUND')) {
                onError?.('Project not found. It may have been deleted.');
                onOpenChange(false);
            } else if (errorMessage.includes('CONCURRENCY_CONFLICT')) {
                setServerError('The project was modified by another process. Please refresh and try again.');
            } else {
                setServerError(errorMessage);
            }
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
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
