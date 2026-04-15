import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useUpdateDefectMutation } from '@/generated/graphql';
import { toast } from 'react-toastify';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

const defectSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    plan: z.string().optional(),
    securityImpact: z.string().optional(),
    performanceImpact: z.string().optional(),
    severity: z.enum(['CRITICAL', 'HIGH', 'MEDIUM', 'LOW']),
});

type DefectFormData = z.infer<typeof defectSchema>;

interface DefectData {
    id: string;
    title: string;
    description: string | null;
    acceptanceCriteria: string | null;
    plan: string | null;
    securityImpact: string | null;
    performanceImpact: string | null;
    severity: string;
    updatedAt: string;
}

interface EditDefectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    defect: DefectData | null;
    onSuccess?: () => void;
}

export function EditDefectDialog({ open, onOpenChange, defect, onSuccess }: EditDefectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [updateDefect, { loading }] = useUpdateDefectMutation();

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        formState: { errors },
    } = useForm<DefectFormData>({
        resolver: zodResolver(defectSchema),
    });

    useEffect(() => {
        if (defect && open) {
            reset({
                title: defect.title,
                description: defect.description ?? '',
                acceptanceCriteria: defect.acceptanceCriteria ?? '',
                plan: defect.plan ?? '',
                securityImpact: defect.securityImpact ?? '',
                performanceImpact: defect.performanceImpact ?? '',
                severity: defect.severity as DefectFormData['severity'],
            });
            setServerError(null);
        }
    }, [defect, open, reset]);

    const onSubmit = async (data: DefectFormData) => {
        if (!defect) return;

        setServerError(null);

        try {
            const result = await updateDefect({
                variables: {
                    input: {
                        id: defect.id,
                        title: data.title,
                        description: data.description ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        plan: data.plan ?? null,
                        securityImpact: data.securityImpact ?? null,
                        performanceImpact: data.performanceImpact ?? null,
                        testPlan: null,
                        deploymentPlan: null,
                        openQuestions: null,
                    },
                },
            });

            const payload = result.data?.updateDefect;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                if (errorMessage.includes('NOT_FOUND')) {
                    toast.error('Defect not found. It may have been deleted.');
                    onOpenChange(false);
                } else {
                    setServerError(errorMessage);
                }
                return;
            }

            toast.success('Defect updated successfully');
            onSuccess?.();
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to update defect');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    if (!defect) return null;

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Defect</DialogTitle>
                    <DialogDescription>
                        Update defect details. Changes will be saved immediately.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Defect title"
                            />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="severity">Severity *</Label>
                            <Select
                                defaultValue={defect.severity}
                                onValueChange={(value) => setValue('severity', value as DefectFormData['severity'])}
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select severity" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="CRITICAL">Critical</SelectItem>
                                    <SelectItem value="HIGH">High</SelectItem>
                                    <SelectItem value="MEDIUM">Medium</SelectItem>
                                    <SelectItem value="LOW">Low</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.severity && (
                                <p className="text-sm text-destructive">{errors.severity.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                {...register('description')}
                                placeholder="Describe the defect"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="Criteria for resolving this defect"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="plan">Plan</Label>
                            <Textarea
                                id="plan"
                                {...register('plan')}
                                placeholder="Plan to address this defect"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="securityImpact">Security Impact</Label>
                            <Textarea
                                id="securityImpact"
                                {...register('securityImpact')}
                                placeholder="Security implications"
                                rows={2}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="performanceImpact">Performance Impact</Label>
                            <Textarea
                                id="performanceImpact"
                                {...register('performanceImpact')}
                                placeholder="Performance implications"
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
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
