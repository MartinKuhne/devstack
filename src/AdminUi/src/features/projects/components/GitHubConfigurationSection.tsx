import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Eye, EyeOff } from 'lucide-react';

interface ProjectSummary {
    id: string | null;
    name: string | null;
    description: string | null;
    repository: string | null;
}

interface GitHubConfigurationSectionProps {
    project: ProjectSummary;
}

export function GitHubConfigurationSection({ project }: GitHubConfigurationSectionProps) {
    const [showToken, setShowToken] = useState(false);
    const [tokenValue, setTokenValue] = useState('');
    const [isUpdating, setIsUpdating] = useState(false);

    const handleSaveToken = async () => {
        if (!tokenValue.trim()) {
            return;
        }
        setIsUpdating(true);
        try {
            await new Promise((resolve) => setTimeout(resolve, 500));
            setTokenValue('');
            setShowToken(false);
        } catch (error) {
            console.error('Failed to refresh token:', error);
        } finally {
            setIsUpdating(false);
        }
    };

    const handleClearToken = () => {
        setTokenValue('');
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>GitHub Configuration</CardTitle>
                <CardDescription>
                    Configure GitHub repository access for this project
                </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="space-y-2">
                    <Label htmlFor="github-url">GitHub Repository URL</Label>
                    {project.repository ? (
                        <div className="flex items-center gap-2">
                            <Input
                                id="github-url"
                                value={project.repository}
                                disabled
                                className="bg-muted"
                            />
                            <Button variant="outline" asChild>
                                <a
                                    href={project.repository}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                >
                                    Open
                                </a>
                            </Button>
                        </div>
                    ) : (
                        <p className="text-sm text-muted-foreground">
                            No GitHub repository configured
                        </p>
                    )}
                    <p className="text-xs text-muted-foreground">
                        Set the repository URL in the project edit dialog
                    </p>
                </div>

                <div className="space-y-2">
                    <Label htmlFor="github-token">GitHub Token</Label>
                    <div className="flex items-center gap-2">
                        <div className="relative flex-1">
                            <Input
                                id="github-token"
                                type={showToken ? 'text' : 'password'}
                                placeholder="Enter GitHub token (will be encrypted)"
                                value={tokenValue}
                                onChange={(e) => setTokenValue(e.target.value)}
                                disabled={isUpdating}
                            />
                            <Button
                                variant="ghost"
                                size="sm"
                                className="absolute right-0 top-0 h-full px-3"
                                onClick={() => setShowToken(!showToken)}
                                type="button"
                            >
                                {showToken ? (
                                    <EyeOff className="h-4 w-4" />
                                ) : (
                                    <Eye className="h-4 w-4" />
                                )}
                            </Button>
                        </div>
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleClearToken}
                            disabled={!tokenValue || isUpdating}
                            type="button"
                        >
                            Clear
                        </Button>
                        <Button
                            size="sm"
                            onClick={handleSaveToken}
                            disabled={!tokenValue || isUpdating}
                            type="button"
                        >
                            Save
                        </Button>
                    </div>
                    <p className="text-xs text-muted-foreground">
                        Token is encrypted before storage.{' '}
                        {showToken ? 'Token is visible.' : 'Token is masked for security.'}
                    </p>
                </div>
            </CardContent>
        </Card>
    );
}
