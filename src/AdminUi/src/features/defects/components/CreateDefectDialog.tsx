import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { gql } from '@apollo/client/core';
import { useMutation, useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { CreateDefectMutation, CreateDefectMutationVariables, GetFeaturesQuery } from '@/generated/graphql';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { cn } from '@/lib';
import { toast } from 'react-toastify';

const CREATE_DEFECT = gql`
    mutation CreateDefect($input: CreateDefectInput!) {
        createDefect(input: $input) {
            id
            title
            severity
            status
        }
    }
`;

const GET_FEATURES = gql`
    query GetFeatures($projectId: ID) {
        features(projectId: $projectId) {
            edges {
                node {
                    id
                    title
                    status
                }
            }
        }
    }
`;

const defectSchema = z.object({
    title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
    description: z.string().optional(),
    acceptanceCriteria: z.string().optional(),
    plan: z.string().optional(),
    securityImpact: z.string().optional(),
    performanceImpact: z.string().optional(),
    severity: z.enum(['Critical', 'High', 'Medium', 'Low']),
    projectId: z.string().min(1, 'Project is required'),
    parentFeatureId: z.string().optional(),
});

type DefectFormData = z.infer<typeof defectSchema>;

interface CreateDefectDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    projectId: string;
    onSuccess?: (defectId: string) => void;
}

export function CreateDefectDialog({ open, onOpenChange, projectId, onSuccess }: CreateDefectDialogProps) {
    const [serverError, setServerError] = useState<string | null>(null);
    const [createDefect, { loading }] = useMutation<CreateDefectMutation, CreateDefectMutationVariables>(CREATE_DEFECT, {
        client: getApolloClient(),
    });

    const { data: featuresData } = useQuery<GetFeaturesQuery>(GET_FEATURES, {
        client: getApolloClient(),
        variables: { projectId },
        skip: !open,
    });

    const {
        register,
        handleSubmit,
        reset,
        setValue,
        watch,
        formState: { errors },
    } = useForm<DefectFormData>({
        resolver: zodResolver(defectSchema),
        defaultValues: {
            severity: 'Medium',
        },
    });

    const parentFeatureId = watch('parentFeatureId');

    useEffect(() => {
        if (open) {
            reset({
                title: '',
                description: '',
                acceptanceCriteria: '',
                plan: '',
                securityImpact: '',
                performanceImpact: '',
                severity: 'Medium',
                projectId: projectId,
                parentFeatureId: '',
            });
            setServerError(null);
        }
    }, [open, projectId, reset]);

    const onSubmit = async (data: DefectFormData) => {
        setServerError(null);
        
        try {
            const result = await createDefect({
                variables: {
                    input: {
                        projectId: data.projectId,
                        parentFeatureId: data.parentFeatureId || null,
                        title: data.title,
                        description: data.description ?? null,
                        acceptanceCriteria: data.acceptanceCriteria ?? null,
                        plan: data.plan ?? null,
                        securityImpact: data.securityImpact ?? null,
                        performanceImpact: data.performanceImpact ?? null,
                        severity: data.severity,
                        result: null,
                        errors: null,
                    },
                },
            });

            reset();
            toast.success('Defect created successfully');
            onSuccess?.(result.data?.createDefect.id ?? '');
            onOpenChange(false);
        } catch (err) {
            setServerError(err instanceof Error ? err.message : 'Failed to create defect');
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setServerError(null);
        }
        onOpenChange(isOpen);
    };

    const features = featuresData?.features.edges?.map(edge => edge.node) || [];

    return (
        <Dialog open={open} onOpenChange={handleOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Defect</DialogTitle>
                    <DialogDescription>
                        Report a new defect. Optionally link it to a parent feature.
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
                                placeholder="Defect title"
                            />
                            {errors.title && (
                                <p className="text-sm text-destructive">{errors.title.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="severity">Severity *</Label>
                            <Select defaultValue="Medium" onValueChange={(value) => setValue('severity', value as any)}>
                                <SelectTrigger>
                                    <SelectValue placeholder="Select severity" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Critical">Critical</SelectItem>
                                    <SelectItem value="High">High</SelectItem>
                                    <SelectItem value="Medium">Medium</SelectItem>
                                    <SelectItem value="Low">Low</SelectItem>
                                </SelectContent>
                            </Select>
                            {errors.severity && (
                                <p className="text-sm text-destructive">{errors.severity.message}</p>
                            )}
                        </div>

                        <div className="grid gap-2">
                            <Label htmlFor="parentFeatureId">Parent Feature (optional)</Label>
                            <Popover>
                                <PopoverTrigger asChild>
                                    <Button variant="outline" role="combobox" className="w-full justify-between">
                                        {parentFeatureId
                                            ? features.find(f => f.id === parentFeatureId)?.title
                                            : 'Select a feature...'}
                                    </Button>
                                </PopoverTrigger>
                                <PopoverContent className="w-full p-0">
                                    <Command>
                                        <CommandInput placeholder="Search features..." />
                                        <CommandList>
                                            <CommandEmpty>No feature found.</CommandEmpty>
                                            <CommandGroup>
                                                {features.map((feature) => (
                                                    <CommandItem
                                                        key={feature.id}
                                                        value={feature.id}
                                                        onSelect={() => {
                                                            setValue('parentFeatureId', feature.id === parentFeatureId ? '' : feature.id);
                                                        }}
                                                    >
                                                        {feature.title}
                                                        <span className={cn(
                                                            'ml-auto',
                                                            feature.id === parentFeatureId ? 'opacity-100' : 'opacity-0'
                                                        )}>✓</span>
                                                    </CommandItem>
                                                ))}
                                            </CommandGroup>
                                        </CommandList>
                                    </Command>
                                </PopoverContent>
                            </Popover>
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
                            {loading ? 'Creating...' : 'Create Defect'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
