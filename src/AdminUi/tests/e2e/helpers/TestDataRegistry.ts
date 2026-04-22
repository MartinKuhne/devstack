import type { Page } from '@playwright/test';

interface CreatedEntity {
    type: 'Project' | 'Deliverable' | 'AgentTask' | 'LargeLanguageModel';
    id: string;
}

export class TestDataRegistry {
    private static entities: CreatedEntity[] = [];
    private static pageInstance: Page | null = null;
    private static apiUrl: string;
    private static interceptionSetup = false;

    constructor() {
        TestDataRegistry.apiUrl = process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';
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

    static setPage(page: Page): void {
        if (!TestDataRegistry.interceptionSetup && page) {
            TestDataRegistry.pageInstance = page;
            TestDataRegistry.setupInterception(page);
            TestDataRegistry.interceptionSetup = true;
        }
    }

    async cleanup(): Promise<void> {
        const entities = [...TestDataRegistry.entities];
        TestDataRegistry.entities = [];

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

        let mutation: string;

        switch (entity.type) {
            case 'Project':
                mutation = `mutation DeleteProject($input: DeleteProjectInput!) { deleteProject(input: $input) { project errors } }`;
                break;
            case 'Deliverable':
                mutation = `mutation DeleteDeliverable($input: DeleteDeliverableInput!) { deleteDeliverable(input: $input) { deliverable errors } }`;
                break;
            case 'AgentTask':
                mutation = `mutation DeleteAgentTask($input: DeleteAgentTaskInput!) { deleteAgentTask(input: $input) { agentTask errors } }`;
                break;
            case 'LargeLanguageModel':
                mutation = `mutation DeleteLargeLanguageModel($input: DeleteLargeLanguageModelInput!) { deleteLargeLanguageModel(input: $input) { largeLanguageModel errors } }`;
                break;
            default:
                return;
        }

        try {
            const response = await fetch(TestDataRegistry.apiUrl, {
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

            const result = (await response.json()) as { errors?: Array<{ message: string }> };

            if (result.errors && result.errors.length > 0) {
                console.warn(`GraphQL errors deleting ${entity.type}:`, result.errors);
            }
        } catch {
            // Ignore cleanup failures - entity may already be deleted
        }
    }

    private static setupInterception(page: Page): void {
        const mutationMapping: Record<
            string,
            { type: CreatedEntity['type']; variableName: string }
        > = {
            createProject: { type: 'Project', variableName: 'project' },
            createDeliverable: { type: 'Deliverable', variableName: 'deliverable' },
            createAgentTask: { type: 'AgentTask', variableName: 'agentTask' },
            createLargeLanguageModel: {
                type: 'LargeLanguageModel',
                variableName: 'largeLanguageModel',
            },
        };

        page.on('response', async (response) => {
            const url = response.url();
            if (!url.includes('/graphql')) {
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
            } catch {
                // Ignore parsing errors
            }
        });
    }
}
