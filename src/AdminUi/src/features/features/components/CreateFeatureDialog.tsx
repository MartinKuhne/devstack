import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useCreateFeatureMutation } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const featureSchema = z.object({
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    openQuestions: z.string().optional(),
    projectId: z.string().min(1, 'Project is required'),
});

type FeatureFormData = z.infer<typeof featureSchema>;

interface CreateFeatureDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    projectId: string;
    onSuccess?: (featureId: string) => void;
}

export function CreateFeatureDialog({ open, onOpenChange, projectId, onSuccess }: CreateFeatureDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createFeature, { loading }] = useCreateFeatureMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<FeatureFormData>({
        resolver: zodResolver(featureSchema),
        defaultValues: {
            projectId: projectId,
        },
    });

    const onSubmit = async (data: FeatureFormData) => {
        setServerError(null);

        try {
            const result = await createFeature({
                variables: {
                    input: {
                        title: data.title,
                        description: data.description ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        openQuestions: data.openQuestions ?? null,
                        projectId: data.projectId,
                        initialStatus: null,
                        deploymentPlan: null,
                        performanceImpact: null,
                        plan: null,
                        securityImpact: null,
                        testPlan: null,
                    },
                },
            });

            const payload = result.data?.createFeature;
            if (payload?.errors?.length) {
                setServerError(payload.errors.join(', '));
                return;
            }

            reset();
            onSuccess?.(payload?.item?.id ?? '');
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to create feature');
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
                    <DialogTitle>Create New Feature</DialogTitle>
                    <DialogDescription>
                        Add a new feature to track development work.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <input type="hidden" {...register('projectId')} value={projectId} />

                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Feature title"
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
                                placeholder="Describe the feature"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="What needs to be done to consider this feature complete?"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="openQuestions">Open Questions</Label>
                            <Textarea
                                id="openQuestions"
                                {...register('openQuestions')}
                                placeholder="Questions that need to be answered"
                                rows={2}
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
                            {loading ? 'Creating...' : 'Create Feature'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
