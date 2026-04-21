import { useState, useEffect } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { useUpdateDeliverableMutation } from '@/generated/graphql';

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
        errors?: string | null;
        result?: string | null;
    } | null;
    onSuccess: () => void;
}

export function EditDeliverableDialog({
    open,
    onOpenChange,
    deliverable,
    onSuccess,
}: EditDeliverableDialogProps) {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [acceptanceCriteria, setAcceptanceCriteria] = useState('');
    const [plan, setPlan] = useState('');
    const [securityImpact, setSecurityImpact] = useState('');
    const [performanceImpact, setPerformanceImpact] = useState('');
    const [testPlan, setTestPlan] = useState('');
   const [deploymentPlan, setDeploymentPlan] = useState('');
    const [openQuestions, setOpenQuestions] = useState('');
    const [errors, setErrors] = useState('');
    const [result, setResult] = useState('');
    const [error, setError] = useState<string | null>(null);

    const [updateDeliverable, { loading }] = useUpdateDeliverableMutation();

    /* eslint-disable react-hooks/set-state-in-effect */
    useEffect(() => {
        if (deliverable) {
            setTitle(deliverable.title ?? '');
            setDescription(deliverable.description ?? '');
            setAcceptanceCriteria(deliverable.acceptanceCriteria ?? '');
            setPlan(deliverable.executionPlan ?? '');
            setSecurityImpact(deliverable.securityImpact ?? '');
            setPerformanceImpact(deliverable.performanceImpact ?? '');
            setTestPlan(deliverable.testPlan ?? '');
            setDeploymentPlan(deliverable.deploymentPlan ?? '');
            setOpenQuestions(deliverable.blocking ?? '');
            setErrors(deliverable.errors ?? '');
            setResult(deliverable.result ?? '');
        }
    }, [deliverable]);
    /* eslint-enable react-hooks/set-state-in-effect */

    const resetForm = () => {
        setTitle('');
        setDescription('');
        setAcceptanceCriteria('');
        setPlan('');
        setSecurityImpact('');
        setPerformanceImpact('');
        setTestPlan('');
        setDeploymentPlan('');
        setOpenQuestions('');
         setErrors('');
        setResult('');
        setError(null);
    };

    const handleOpenChange = (newOpen: boolean) => {
        setError(null);
        if (!newOpen) {
            resetForm();
        }
        onOpenChange(newOpen);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!deliverable?.id) {
            setError('Deliverable ID is required');
            return;
        }

        if (!title.trim()) {
            setError('Title is required');
            return;
        }

        try {
            const mutationResult = await updateDeliverable({
                variables: {
                    input: {
                        id: deliverable.id,
                        title,
                        description: description || null,
                        acceptanceCriteria: acceptanceCriteria || null,
                        plan: plan || null,
                        securityImpact: securityImpact || null,
                        performanceImpact: performanceImpact || null,
                        testPlan: testPlan || null,
                        deploymentPlan: deploymentPlan || null,
                        openQuestions: openQuestions || null,
                        errors: errors || null,
                        result: result || null,
                    },
                },
            });

            const payload = mutationResult.data?.updateDeliverable;
            if (payload?.errors?.length) {
                setError(payload.errors.join(', '));
                return;
            }

            resetForm();
            onOpenChange(false);
            onSuccess();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'An unexpected error occurred');
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
                    <DialogDescription>
                        Update the deliverable details.
                    </DialogDescription>
                </DialogHeader>
                <form onSubmit={handleSubmit}>
                    <div className="grid gap-4 py-4">
                        {error && (
                            <div className="text-sm text-destructive">{error}</div>
                        )}
                        <div className="grid gap-2">
                            <Label htmlFor="title">Title</Label>
                            <Input
                                id="title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                required
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                            <Textarea
                                id="acceptanceCriteria"
                                value={acceptanceCriteria}
                                onChange={(e) => setAcceptanceCriteria(e.target.value)}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="plan">Execution Plan</Label>
                            <Textarea
                                id="plan"
                                value={plan}
                                onChange={(e) => setPlan(e.target.value)}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="securityImpact">Security Impact</Label>
                            <Textarea
                                id="securityImpact"
                                value={securityImpact}
                                onChange={(e) => setSecurityImpact(e.target.value)}
                                rows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="performanceImpact">Performance Impact</Label>
                            <Textarea
                                id="performanceImpact"
                                value={performanceImpact}
                                onChange={(e) => setPerformanceImpact(e.target.value)}
                                rows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="testPlan">Test Plan</Label>
                            <Textarea
                                id="testPlan"
                                value={testPlan}
                                onChange={(e) => setTestPlan(e.target.value)}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="deploymentPlan">Deployment Plan</Label>
                            <Textarea
                                id="deploymentPlan"
                                value={deploymentPlan}
                                onChange={(e) => setDeploymentPlan(e.target.value)}
                                rows={3}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="openQuestions">Open Questions</Label>
                            <Textarea
                                id="openQuestions"
                                value={openQuestions}
                                onChange={(e) => setOpenQuestions(e.target.value)}
                                rows={2}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="errors">Errors</Label>
                            <Textarea
                                id="errors"
                                value={errors}
                                onChange={(e) => setErrors(e.target.value)}
                                rows={2}
                            />
                        </div>
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
