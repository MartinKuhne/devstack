import { useState, useCallback } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { LargeLanguageModelList } from '../components/LargeLanguageModelList';
import { LargeLanguageModelDialog } from '../components/LargeLanguageModelDialog';
import { useLargeLanguageModels } from '../hooks/useLargeLanguageModels';

export function LargeLanguageModelsPage() {
    const [addDialogOpen, setAddDialogOpen] = useState(false);
    const { refetch } = useLargeLanguageModels();

    const handleRefetch = useCallback(() => {
        refetch();
    }, [refetch]);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">Large Language Models</h2>
                    <p className="text-muted-foreground">Configure AI model endpoints for the automation system.</p>
                </div>
                <Button onClick={() => setAddDialogOpen(true)}>Add Model</Button>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Model Configurations</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                    <LargeLanguageModelList
                        onAddModel={() => setAddDialogOpen(true)}
                        onRefetch={handleRefetch}
                    />
                </CardContent>
            </Card>

            <LargeLanguageModelDialog
                open={addDialogOpen}
                onOpenChange={setAddDialogOpen}
                onSuccess={handleRefetch}
            />
        </div>
    );
}
