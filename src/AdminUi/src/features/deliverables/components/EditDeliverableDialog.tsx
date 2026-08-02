import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
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

const deliverableSchema = z.object({
    title: z.string().min(1, 'Title is required').max(300, 'Title must be 300 characters or less'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    executionPlan: z.string().optional(),
    securityImpact: z.string().optional(),
    performanceImpact: z.string().optional(),
    testPlan: z.string().optional(),
    deploymentPlan: z.string().optional(),
    blocking: z.string().optional(),
    design: z.string().optional(),
});

type EditDeliverableFormData = z.infer<typeof deliverableSchema>;

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
        design?: string | null;
    } | null;
    onSuccess: () => void;
}

export function EditDeliverableDialog({
    open,
    onOpenChange,
    deliverable,
    onSuccess,
}: EditDeliverableDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isValid },
    } = useForm<EditDeliverableFormData>({
        resolver: zodResolver(deliverableSchema),
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
            design: '',
        },
    });

    useEffect(() => {
        if (deliverable && open) {
            reset({
                title: deliverable.title ?? '',
                description: deliverable.description ?? '',
                acceptanceCriteria: deliverable.acceptanceCriteria ?? '',
                executionPlan: deliverable.executionPlan ?? '',
                securityImpact: deliverable.securityImpact ?? '',
                performanceImpact: deliverable.performanceImpact ?? '',
                testPlan: deliverable.testPlan ?? '',
                deploymentPlan: deliverable.deploymentPlan ?? '',
                blocking: deliverable.blocking ?? '',
                design: deliverable.design ?? '',
            });
        }
    }, [deliverable, open, reset]);

    const [updateDeliverable, { loading }] = useUpdateDeliverableMutation();

    const handleOpenChange = (newOpen: boolean) => {
        if (!newOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(newOpen);
    };

    const onSubmit = async (data: EditDeliverableFormData) => {
        if (!deliverable?.id) {
            return;
        }

        setServerError(null);
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
                        design: data.design || null,
                    },
                },
            });

            const result = mutationResult.data?.updateDeliverable;
            if (!result) {
                logger.warn('Failed to update deliverable', {
                    id: deliverable.id,
                });
                setServerError('Failed to update deliverable');
                return;
            }

            logger.info('Deliverable updated successfully', {
                id: deliverable.id,
                title: data.title,
            });
            reset();
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
        }
    };

    if (!deliverable) {
        return null;
    }

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-4xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Deliverable</DialogTitle>
                    <DialogDescription>Update the deliverable details.</DialogDescription>
                </DialogHeader>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        {serverError && <p className="text-sm text-destructive">{serverError}</p>}
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input id="title" {...register('title')} placeholder="Deliverable title" />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
                            )}
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea id="description" {...register('description')} minRows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                minRows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="executionPlan">Execution Plan</Label>
                            <Textarea id="executionPlan" {...register('executionPlan')} minRows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="securityImpact">Security Impact</Label>
                            <Textarea
                                id="securityImpact"
                                {...register('securityImpact')}
                                minRows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="performanceImpact">Performance Impact</Label>
                            <Textarea
                                id="performanceImpact"
                                {...register('performanceImpact')}
                                minRows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="testPlan">Test Plan</Label>
                            <Textarea id="testPlan" {...register('testPlan')} minRows={3} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="deploymentPlan">Deployment Plan</Label>
                            <Textarea
                                id="deploymentPlan"
                                {...register('deploymentPlan')}
                                minRows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="blocking">Blocking</Label>
                            <Textarea id="blocking" {...register('blocking')} minRows={2} />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="design">Design</Label>
                            <Textarea id="design" {...register('design')} minRows={3} />
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
                        <Button type="submit" disabled={!isValid || loading}>
                            {loading ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
