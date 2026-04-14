import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

interface GlobalSettings {
    pollingInterval: number;
    themePreference: 'light' | 'dark' | 'system';
    defaultModelPerWorkflow: Record<string, string>;
}

const POLLING_INTERVALS = [
    { value: 15, label: '15 seconds' },
    { value: 30, label: '30 seconds' },
    { value: 60, label: '1 minute' },
    { value: 120, label: '2 minutes' },
    { value: 300, label: '5 minutes' },
];

const THEMES = [
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
    { value: 'system', label: 'System' },
];

export function SettingsPage() {
    const [settings, setSettings] = useState<GlobalSettings>({
        pollingInterval: 30,
        themePreference: 'system',
        defaultModelPerWorkflow: {},
    });
    const [saved, setSaved] = useState(false);

    useEffect(() => {
        const stored = localStorage.getItem('globalSettings');
        if (stored) {
            try {
                const parsed = JSON.parse(stored);
                setSettings(parsed);
            } catch {
                // Ignore parse errors
            }
        }
    }, []);

    useEffect(() => {
        if (saved) {
            const timeout = setTimeout(() => setSaved(false), 2000);
            return () => {
                clearTimeout(timeout);
            };
        }
        return undefined;
    }, [saved]);

    const handleSave = () => {
        localStorage.setItem('globalSettings', JSON.stringify(settings));
        setSaved(true);
    };

    const handleReset = () => {
        setSettings({
            pollingInterval: 30,
            themePreference: 'system',
            defaultModelPerWorkflow: {},
        });
        localStorage.removeItem('globalSettings');
        setSaved(true);
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-2xl font-bold tracking-tight">Settings</h2>
                <p className="text-muted-foreground">Global application preferences.</p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Appearance</CardTitle>
                    <CardDescription>Customize the look and feel of the application.</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="theme">Theme Preference</Label>
                        <Select
                            value={settings.themePreference}
                            onValueChange={(value) =>
                                setSettings((s) => ({ ...s, themePreference: value as 'light' | 'dark' | 'system' }))
                            }
                        >
                            <SelectTrigger id="theme">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                {THEMES.map((theme) => (
                                    <SelectItem key={theme.value} value={theme.value}>
                                        {theme.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                        <p className="text-sm text-muted-foreground">
                            {settings.themePreference === 'system'
                                ? 'Automatically match your system theme'
                                : `Use ${settings.themePreference} theme`}
                        </p>
                    </div>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Automation</CardTitle>
                    <CardDescription>Configure background refresh and polling behavior.</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="pollingInterval">Auto-refresh Interval</Label>
                        <Select
                            value={settings.pollingInterval.toString()}
                            onValueChange={(value) =>
                                setSettings((s) => ({ ...s, pollingInterval: parseInt(value, 10) }))
                            }
                        >
                            <SelectTrigger id="pollingInterval">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                {POLLING_INTERVALS.map((interval) => (
                                    <SelectItem key={interval.value} value={interval.value.toString()}>
                                        {interval.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                        <p className="text-sm text-muted-foreground">
                            Dashboard and list pages will automatically refresh every {settings.pollingInterval} seconds
                        </p>
                    </div>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Default Models</CardTitle>
                    <CardDescription>
                        Configure default LLM models for different workflow types.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="plannerModel">Planner Workflow</Label>
                        <Input
                            id="plannerModel"
                            placeholder="e.g., gpt-4o-mini"
                            value={settings.defaultModelPerWorkflow['planner'] ?? ''}
                            onChange={(e) =>
                                setSettings((s) => ({
                                    ...s,
                                    defaultModelPerWorkflow: {
                                        ...s.defaultModelPerWorkflow,
                                        planner: e.target.value,
                                    },
                                }))
                            }
                        />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="devleadModel">DevLead Workflow</Label>
                        <Input
                            id="devleadModel"
                            placeholder="e.g., gpt-4o"
                            value={settings.defaultModelPerWorkflow['devlead'] ?? ''}
                            onChange={(e) =>
                                setSettings((s) => ({
                                    ...s,
                                    defaultModelPerWorkflow: {
                                        ...s.defaultModelPerWorkflow,
                                        devlead: e.target.value,
                                    },
                                }))
                            }
                        />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="coderModel">Coder Workflow</Label>
                        <Input
                            id="coderModel"
                            placeholder="e.g., claude-3-5-sonnet"
                            value={settings.defaultModelPerWorkflow['coder'] ?? ''}
                            onChange={(e) =>
                                setSettings((s) => ({
                                    ...s,
                                    defaultModelPerWorkflow: {
                                        ...s.defaultModelPerWorkflow,
                                        coder: e.target.value,
                                    },
                                }))
                            }
                        />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="testerModel">Tester Workflow</Label>
                        <Input
                            id="testerModel"
                            placeholder="e.g., gpt-4o"
                            value={settings.defaultModelPerWorkflow['tester'] ?? ''}
                            onChange={(e) =>
                                setSettings((s) => ({
                                    ...s,
                                    defaultModelPerWorkflow: {
                                        ...s.defaultModelPerWorkflow,
                                        tester: e.target.value,
                                    },
                                }))
                            }
                        />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="architectModel">Architect Workflow</Label>
                        <Input
                            id="architectModel"
                            placeholder="e.g., claude-3-5-sonnet"
                            value={settings.defaultModelPerWorkflow['architect'] ?? ''}
                            onChange={(e) =>
                                setSettings((s) => ({
                                    ...s,
                                    defaultModelPerWorkflow: {
                                        ...s.defaultModelPerWorkflow,
                                        architect: e.target.value,
                                    },
                                }))
                            }
                        />
                    </div>
                </CardContent>
            </Card>

            <div className="flex items-center gap-4">
                <Button onClick={handleSave}>Save Changes</Button>
                <Button variant="outline" onClick={handleReset}>
                    Reset to Defaults
                </Button>
                {saved && <span className="text-sm text-green-600">Settings saved!</span>}
            </div>
        </div>
    );
}
