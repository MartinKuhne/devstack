import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useCreateDeliverableMutation } from '@/generated/graphql';
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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';

const logger = createModuleLogger('CreateDeliverableDialog');

const deliverableSchema = z.object({
    title: z.string().min(1, 'Title is required').max(300, 'Title must be 300 characters or less'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    securityImpact: z.string().optional(),
    performanceImpact: z.string().optional(),
    testPlan: z.string().optional(),
    deploymentPlan: z.string().optional(),
    type: z.enum(['FEATURE', 'DEFECT', 'MAINTENANCE']),
    initialStatus: z.enum(['DRAFT', 'PLANNING', 'READY']).optional(),
});

type DeliverableFormData = z.infer<typeof deliverableSchema>;

interface CreateDeliverableDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: (deliverableId: string) => void;
}

export function CreateDeliverableDialog({
    open,
    onOpenChange,
    onSuccess,
}: CreateDeliverableDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createDeliverable, { loading }] = useCreateDeliverableMutation();

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        formState: { errors, isValid },
    } = useForm<DeliverableFormData>({
        resolver: zodResolver(deliverableSchema),
        defaultValues: {
            type: 'FEATURE',
            initialStatus: 'DRAFT',
        },
    });

    const onSubmit = async (data: DeliverableFormData) => {
        setServerError(null);
        logger.info('Creating deliverable', { type: data.type, title: data.title });

        try {
            const result = await createDeliverable({
                variables: {
                    input: {
                        projectId: '',
                        type: data.type,
                        title: data.title,
                        description: data.description ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        securityImpact: data.securityImpact ?? null,
                        performanceImpact: data.performanceImpact ?? null,
                        testPlan: data.testPlan ?? null,
                        deploymentPlan: data.deploymentPlan ?? null,
                        initialStatus: data.initialStatus ?? null,
                    },
                },
            });

            const payload = result.data?.createDeliverable;
            if (payload?.errors?.length) {
                const errorMessage = payload.errors.join(', ');
                logger.warn('Failed to create deliverable', {
                    type: data.type,
                    title: data.title,
                    errors: payload.errors,
                });
                setServerError(errorMessage);
                return;
            }

            logger.info('Deliverable created successfully', {
                id: payload?.deliverable?.id,
                title: data.title,
            });
            reset();
            onSuccess?.(payload?.deliverable?.id ?? '');
            onOpenChange(false);
        } catch (err) {
            const errorInfo = formatGraphQLError(err);
            logger.error('Failed to create deliverable', {
                type: data.type,
                title: data.title,
                error: errorInfo.message,
                details: errorInfo.details,
            });
            setServerError(err instanceof Error ? err.message : 'Failed to create deliverable');
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
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Deliverable</DialogTitle>
                    <DialogDescription>
                        Add a new deliverable (feature, defect, or maintenance item).
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="grid gap-2">
                            <Label htmlFor="type">Type *</Label>
                            <Select
                                defaultValue="FEATURE"
                                onValueChange={(value) =>
                                    setValue('type', value as DeliverableFormData['type'])
                                }
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select type" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="FEATURE">Feature</SelectItem>
                                    <SelectItem value="DEFECT">Defect</SelectItem>
                                    <SelectItem value="MAINTENANCE">Maintenance</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.type && (
                                <p className="text-sm text-destructive">{errors.type.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="title">Title *</Label>
                            <Input
                                id="title"
                                {...register('title')}
                                placeholder="Deliverable title"
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
                                placeholder="Description of the deliverable"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                {...register('acceptanceCriteria')}
                                placeholder="Criteria for completion"
                                rows={3}
                            />
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="initialStatus">Initial Status</Label>
                            <Select
                                defaultValue="DRAFT"
                                onValueChange={(value) =>
                                    setValue(
                                        'initialStatus',
                                        value as DeliverableFormData['initialStatus']
                                    )
                                }
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select status" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="DRAFT">Draft</SelectItem>
                                    <SelectItem value="PLANNING">Planning</SelectItem>
                                    <SelectItem value="READY">Ready</SelectItem>
                                </SelectContent>
                            </Select>
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
                            {loading ? 'Creating...' : 'Create Deliverable'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
