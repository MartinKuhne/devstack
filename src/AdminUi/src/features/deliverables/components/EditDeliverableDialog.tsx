import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
    DialogDescription,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { useUpdateDeliverableMutation } from '@/generated/graphql';
import { createModuleLogger, formatGraphQLError } from '@/lib/logging';

const logger = createModuleLogger('EditDeliverableDialog');

interface EditDeliverableFormData {
    title: string;
    description: string;
    acceptanceCriteria: string;
    executionPlan: string;
    securityImpact: string;
    performanceImpact: string;
    testPlan: string;
    deploymentPlan: string;
    blocking: string;
}

interface EditDeliverableDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    deliverable: {
        id: string;
        title: string;
        description?: string | null;
        acceptanceCriteria?: string | null;
        executionPlan?: string | null;
        securityImpact?: string | null;
        performanceImpact?: string | null;
        testPlan?: string | null;
        deploymentPlan?: string | null;
        blocking?: string | null;
    } | null;
    onSuccess: () => void;
}

export function EditDeliverableDialog({
    open,
    onOpenChange,
    deliverable,
    onSuccess,
}: EditDeliverableDialogProps) {
    const [submitting, setSubmitting] = useState(false);
    const [serverError, setServerError] = useState<string | null>(null);

    const {
        register,
        handleSubmit: formHandleSubmit,
        setValue,
        reset,
    } = useForm<EditDeliverableFormData>({
        defaultValues: {
            title: '',
            description: '',
            acceptanceCriteria: '',
            executionPlan: '',
            securityImpact: '',
            performanceImpact: '',
            testPlan: '',
            deploymentPlan: '',
            blocking: '',
        },
    });

    useEffect(() => {
        if (deliverable && open) {
            setValue('title', deliverable.title ?? '');
            setValue('description', deliverable.description ?? '');
            setValue('acceptanceCriteria', deliverable.acceptanceCriteria ?? '');
            setValue('executionPlan', deliverable.executionPlan ?? '');
            setValue('securityImpact', deliverable.securityImpact ?? '');
            setValue('performanceImpact', deliverable.performanceImpact ?? '');
            setValue('testPlan', deliverable.testPlan ?? '');
            setValue('deploymentPlan', deliverable.deploymentPlan ?? '');
            setValue('blocking', deliverable.blocking ?? '');
        }
    }, [deliverable, open, setValue]);

    const [updateDeliverable] = useUpdateDeliverableMutation();

    const resetForm = () => {
        reset();
    };

    const handleOpenChange = (newOpen: boolean) => {
        if (!newOpen) {
            resetForm();
            setServerError(null);
        }
        onOpenChange(newOpen);
    };

    const handleSubmit = async (data: EditDeliverableFormData) => {
        if (!deliverable?.id) {
            return;
        }

        if (!data.title.trim()) {
            return;
        }

        setSubmitting(true);
        logger.info('Updating deliverable', { id: deliverable.id, title: data.title });

        try {
            const mutationResult = await updateDeliverable({
                variables: {
                    input: {
                        id: deliverable.id,
                        title: data.title,
                        description: data.description || null,
                        acceptanceCriteria: data.acceptanceCriteria || null,
                        executionPlan: data.executionPlan || null,
                        securityImpact: data.securityImpact || null,
                        performanceImpact: data.performanceImpact || null,
                        testPlan: data.testPlan || null,
                        deploymentPlan: data.deploymentPlan || null,
                        blocking: data.blocking || null,
                    },
                },
            });

            const payload = mutationResult.data?.updateDeliverable;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                logger.warn('Failed to update deliverable', {
                    id: deliverable.id,
                    errors: payload.errors,
                });
                setServerError(errorMessage);
                setSubmitting(false);
                return;
            }

            logger.info('Deliverable updated successfully', {
                id: deliverable.id,
                title: data.title,
            });
            resetForm();
            onOpenChange(false);
            onSuccess();
        } catch (error) {
            const errorInfo = formatGraphQLError(error);
            logger.error('Failed to update deliverable', {
                id: deliverable.id,
                title: data.title,
                error: errorInfo.message,
                details: errorInfo.details,
            });
            setServerError(errorInfo.message || 'Failed to update deliverable');
        } finally {
            setSubmitting(false);
        }
    };

    if (!deliverable) {
        return null;
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px]">
                <DialogHeader>
                    <DialogTitle>Edit Deliverable</DialogTitle>
                    <DialogDescription>Update the deliverable details.</DialogDescription>
                </DialogHeader>
                <form onSubmit={formHandleSubmit(handleSubmit)}>
                    <div className="grid gap-4 py-4">
                        {serverError && <p className="text-sm text-destructive">{serverError}</p>}
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title</Label>
                            <Input id="title" {...register('title')} required />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea id="description" {...register('description')} rows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="executionPlan">Execution Plan</Label>
                            <Textarea id="executionPlan" {...register('executionPlan')} rows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="securityImpact">Security Impact</Label>
                            <Textarea
                                id="securityImpact"
                                {...register('securityImpact')}
                                rows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="performanceImpact">Performance Impact</Label>
                            <Textarea
                                id="performanceImpact"
                                {...register('performanceImpact')}
                                rows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="testPlan">Test Plan</Label>
                            <Textarea id="testPlan" {...register('testPlan')} rows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="deploymentPlan">Deployment Plan</Label>
                            <Textarea
                                id="deploymentPlan"
                                {...register('deploymentPlan')}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="blocking">Blocking</Label>
                            <Textarea id="blocking" {...register('blocking')} rows={2} />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => handleOpenChange(false)}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={submitting}>
                            {submitting ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
