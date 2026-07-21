import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertCircle, RefreshCw, Copy, Check } from 'lucide-react';
import { createModuleLogger } from '@/lib/logging';

const errorBoundaryLogger = createModuleLogger('ErrorBoundary');

interface Props {
    children: ReactNode;
    fallback?: ReactNode;
    name?: string;
}

interface State {
    hasError: boolean;
    error: Error | null;
    errorInfo: ErrorInfo | null;
    copied: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
    public state: State = {
        hasError: false,
        error: null,
        errorInfo: null,
        copied: false,
    };

    public static getDerivedStateFromError(error: Error): State {
        return { hasError: true, error, errorInfo: null, copied: false };
    }

    public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
        this.setState({ errorInfo });

        errorBoundaryLogger.error('Uncaught error caught by ErrorBoundary', {
            componentName: this.props.name,
            error: error.message,
            stack: error.stack,
            componentStack: errorInfo.componentStack,
        });
    }

    private handleReload = () => {
        this.setState({ hasError: false, error: null, errorInfo: null });
        window.location.reload();
    };

    private handleReset = () => {
        this.setState({ hasError: false, error: null, errorInfo: null });
    };

    private handleCopyError = () => {
        const { error, errorInfo } = this.state;
        const errorReport = [
            'Error Boundary Report',
            '====================',
            `Component: ${this.props.name || 'Unknown'}`,
            `Time: ${new Date().toISOString()}`,
            '',
            'Error:',
            error?.message,
            error?.stack,
            '',
            'Component Stack:',
            errorInfo?.componentStack,
        ].join('\n');

        navigator.clipboard.writeText(errorReport).then(() => {
            this.setState({ copied: true });
            setTimeout(() => this.setState({ copied: false }), 2000);
        }).catch(() => {
            errorBoundaryLogger.warn('Failed to copy error to clipboard');
        });
    };

    public render() {
        if (this.state.hasError) {
            if (this.props.fallback) {
                return this.props.fallback;
            }

            const { error, errorInfo } = this.state;

            return (
                <div className="flex items-center justify-center min-h-screen bg-background">
                    <Card className="w-full max-w-2xl">
                        <CardHeader>
                            <div className="flex items-center gap-2 text-destructive">
                                <AlertCircle className="h-6 w-6" />
                                <CardTitle className="text-xl">Something went wrong</CardTitle>
                            </div>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <p className="text-sm text-muted-foreground">
                                An unexpected error occurred in the{' '}
                                <code className="px-1 py-0.5 bg-muted rounded text-sm">
                                    {this.props.name || 'application'}
                                </code>{' '}
                                component. This has been logged for the system administrator.
                            </p>

                            <div className="p-4 bg-destructive/10 rounded-lg border border-destructive/20">
                                <p className="text-sm font-medium text-destructive mb-2">
                                    Error Details
                                </p>
                                <p className="text-sm text-destructive font-mono break-all">
                                    {error?.message || 'Unknown error'}
                                </p>
                            </div>

                            {errorInfo && import.meta.env.DEV && (
                                <details className="p-4 bg-muted/50 rounded-lg">
                                    <summary className="text-sm font-medium cursor-pointer">
                                        Component Stack Trace
                                    </summary>
                                    <pre className="text-xs text-muted-foreground mt-2 overflow-auto max-h-48 whitespace-pre-wrap">
                                        {errorInfo.componentStack}
                                    </pre>
                                </details>
                            )}

                            <div className="flex gap-2">
                                <Button onClick={this.handleReload} className="flex-1">
                                    <RefreshCw className="h-4 w-4 mr-2" />
                                    Reload Page
                                </Button>
                                <Button
                                    variant="outline"
                                    onClick={this.handleReset}
                                    className="flex-1"
                                >
                                    Reset
                                </Button>
                                <Button variant="outline" onClick={this.handleCopyError}>
                                    {this.state.copied ? (
                                        <Check className="h-4 w-4 mr-2" />
                                    ) : (
                                        <Copy className="h-4 w-4 mr-2" />
                                    )}
                                    {this.state.copied ? 'Copied!' : 'Copy Error'}
                                </Button>
                            </div>

                            <p className="text-xs text-muted-foreground">
                                If the problem persists, please contact support with the error
                                details above.
                            </p>
                        </CardContent>
                    </Card>
                </div>
            );
        }

        return this.props.children;
    }
}
