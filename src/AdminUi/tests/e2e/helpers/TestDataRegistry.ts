import type { Page } from '@playwright/test';

interface CreatedEntity {
    type: 'Project' | 'Feature' | 'Defect' | 'Task' | 'Epic' | 'ModelConfiguration';
    id: string;
}

export class TestDataRegistry {
    private static entities: CreatedEntity[] = [];
    private static pageInstance: Page | null = null;
    private static apiUrl: string;
    private static apiKey: string | undefined;
    private static interceptionSetup = false;

    constructor() {
        this.apiUrl = process.env.API_URL || 'http://localhost:5173';
        this.apiKey = process.env.TEST_API_KEY;
    }

    static registerEntity(type: CreatedEntity['type'], id: string): void {
        this.entities.push({ type, id });
    }

    static getEntities(): CreatedEntity[] {
        return [...this.entities];
    }

    static clear(): void {
        this.entities = [];
    }

    static getEntityCount(): number {
        return this.entities.length;
    }

    static setPage(page: Page): void {
        if (!TestDataRegistry.interceptionSetup && page) {
            TestDataRegistry.pageInstance = page;
            TestDataRegistry.setupInterception(page);
            TestDataRegistry.interceptionSetup = true;
        }
    }

    async cleanup(): Promise<void> {
        const entities = [...this.entities];
        this.entities = [];

        for (const entity of entities.reverse()) {
            try {
                await this.deleteEntity(entity);
            } catch (error) {
                console.warn(`Failed to delete ${entity.type} ${entity.id}:`, error);
            }
        }
    }

    private async deleteEntity(entity: CreatedEntity): Promise<void> {
        const headers: Record<string, string> = {
            'Content-Type': 'application/json',
        };

        if (this.apiKey) {
            headers['Authorization'] = `Bearer ${this.apiKey}`;
        }

        let mutation: string;

        switch (entity.type) {
            case 'Project':
                mutation = `mutation DeleteProject($input: DeleteProjectInput!) { deleteProject(input: $input) { project errors } }`;
                break;
            case 'Feature':
                mutation = `mutation DeleteFeature($input: DeleteFeatureInput!) { deleteFeature(input: $input) { feature errors } }`;
                break;
            case 'Defect':
                mutation = `mutation DeleteDefect($input: DeleteDefectInput!) { deleteDefect(input: $input) { defect errors } }`;
                break;
            case 'Task':
                mutation = `mutation DeleteTask($input: DeleteTaskInput!) { deleteTask(input: $input) { task errors } }`;
                break;
            case 'Epic':
                mutation = `mutation DeleteEpic($input: DeleteEpicInput!) { deleteEpic(input: $input) { epic errors } }`;
                break;
            case 'ModelConfiguration':
                mutation = `mutation DeleteModelConfiguration($input: DeleteModelConfigurationInput!) { deleteModelConfiguration(input: $input) { modelConfiguration errors } }`;
                break;
            default:
                return;
        }

        const response = await fetch(`${this.apiUrl}/api/graphql`, {
            method: 'POST',
            headers,
            body: JSON.stringify({
                query: mutation,
                variables: {
                    input: { id: entity.id },
                },
            }),
        });

        if (!response.ok) {
            throw new Error(`Failed to delete ${entity.type}: ${response.statusText}`);
        }

        const result = await response.json();
        
        if (result.errors && result.errors.length > 0) {
            console.warn(`GraphQL errors deleting ${entity.type}:`, result.errors);
        }
    }

    private static setupInterception(page: Page): void {
        const mutationMapping: Record<string, { type: CreatedEntity['type']; variableName: string }> = {
            createProject: { type: 'Project', variableName: 'project' },
            createFeature: { type: 'Feature', variableName: 'feature' },
            createDefect: { type: 'Defect', variableName: 'defect' },
            createTask: { type: 'Task', variableName: 'task' },
            createEpic: { type: 'Epic', variableName: 'epic' },
            createModelConfiguration: { type: 'ModelConfiguration', variableName: 'modelConfiguration' },
        };

        page.on('response', async (response) => {
            const url = response.url();
            if (!url.includes('/api/graphql')) {
                return;
            }

            try {
                const json = await response.json();
                const data = json.data;

                if (!data) {
                    return;
                }

                for (const [mutationName, mapping] of Object.entries(mutationMapping)) {
                    const payload = data[mutationName];
                    if (payload && payload[mapping.variableName]?.id) {
                        const id = payload[mapping.variableName].id;
                        TestDataRegistry.entities.push({ type: mapping.type, id });
                        console.log(`Registered ${mapping.type} for cleanup: ${id}`);
                    }
                }
            } catch (error) {
                // Ignore parsing errors
            }
        });
    }
}
