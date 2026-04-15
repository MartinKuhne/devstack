import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateFeatureMutation } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';

const featureSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    plan: z.string().optional(),
    securityImpact: z.string().optional(),
    performanceImpact: z.string().optional(),
    testPlan: z.string().optional(),
    deploymentPlan: z.string().optional(),
    openQuestions: z.string().optional(),
});

type FeatureFormData = z.infer<typeof featureSchema>;

interface FeatureData {
    id: string;
    title: string;
    description: string | null;
    acceptanceCriteria: string | null;
    plan: string | null;
    securityImpact: string | null;
    performanceImpact: string | null;
    testPlan: string | null;
    deploymentPlan: string | null;
    openQuestions: string | null;
    updatedAt: string;
}

interface EditFeatureDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    feature: FeatureData | null;
    onSuccess?: () => void;
    onError?: (error: string) => void;
}

export function EditFeatureDialog({ open, onOpenChange, feature, onSuccess, onError }: EditFeatureDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateFeature, { loading }] = useUpdateFeatureMutation();

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<FeatureFormData>({
        resolver: zodResolver(featureSchema),
    });

    useEffect(() => {
        if (feature && open) {
            reset({
                title: feature.title,
                description: feature.description ?? '',
                acceptanceCriteria: feature.acceptanceCriteria ?? '',
                plan: feature.plan ?? '',
                securityImpact: feature.securityImpact ?? '',
                performanceImpact: feature.performanceImpact ?? '',
                testPlan: feature.testPlan ?? '',
                deploymentPlan: feature.deploymentPlan ?? '',
                openQuestions: feature.openQuestions ?? '',
            });
        }
    }, [feature, open, reset]);

    const onSubmit = async (data: FeatureFormData) => {
        if (!feature) return;

        setServerError(null);

        try {
            const result = await updateFeature({
                variables: {
                    input: {
                        id: feature.id,
                        title: data.title,
                        description: data.description ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        plan: data.plan ?? null,
                        securityImpact: data.securityImpact ?? null,
                        performanceImpact: data.performanceImpact ?? null,
                        testPlan: data.testPlan ?? null,
                        deploymentPlan: data.deploymentPlan ?? null,
                        openQuestions: data.openQuestions ?? null,
                    },
                },
            });

            const payload = result.data?.updateFeature;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                if (errorMessage.includes('NOT_FOUND')) {
                    onError?.('Feature not found. It may have been deleted.');
                    onOpenChange(false);
                } else {
                    setServerError(errorMessage);
                }
                return;
            }

            reset();
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to update feature');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    if (!feature) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Feature</DialogTitle>
                    <DialogDescription>
                        Update feature details. Changes will be saved immediately.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
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
                                rows={4}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="What needs to be done to consider this feature complete?"
                                rows={4}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="plan">Plan</Label>
                            <Textarea
                                id="plan"
                                {...register('plan')}
                                placeholder="Implementation plan"
                                rows={4}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="securityImpact">Security Impact</Label>
                            <Textarea
                                id="securityImpact"
                                {...register('securityImpact')}
                                placeholder="Security considerations and impacts"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="performanceImpact">Performance Impact</Label>
                            <Textarea
                                id="performanceImpact"
                                {...register('performanceImpact')}
                                placeholder="Performance considerations and impacts"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="testPlan">Test Plan</Label>
                            <Textarea
                                id="testPlan"
                                {...register('testPlan')}
                                placeholder="Testing strategy and plan"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="deploymentPlan">Deployment Plan</Label>
                            <Textarea
                                id="deploymentPlan"
                                {...register('deploymentPlan')}
                                placeholder="Deployment strategy and notes"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="openQuestions">Open Questions</Label>
                            <Textarea
                                id="openQuestions"
                                {...register('openQuestions')}
                                placeholder="Questions that need to be answered"
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
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
