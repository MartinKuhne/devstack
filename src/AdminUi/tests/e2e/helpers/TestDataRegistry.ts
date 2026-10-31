import type { Page } from '@playwright/test';
import { deleteEntityById } from './testDataOperations.js';

interface CreatedEntity {
    type: 'Project' | 'Deliverable' | 'AgentTask' | 'LargeLanguageModel';
    id: string;
}

/**
 * Mutation field per entity type. The schema signature is
 * `delete<Entity>(id: UUID!): Boolean!` - there is no `Delete<Entity>Input`.
 */
const DELETE_FIELD_BY_TYPE: Record<CreatedEntity['type'], Parameters<typeof deleteEntityById>[1]> =
    {
        Project: 'deleteProject',
        Deliverable: 'deleteDeliverable',
        AgentTask: 'deleteAgentTask',
        LargeLanguageModel: 'deleteLargeLanguageModel',
    };

/**
 * Create mutations whose responses the harness watches so that anything the UI
 * creates during a test is deleted again. Each of these mutations returns the entity
 * itself (e.g. `createProject(input: ...) { id ... }`), not a wrapper payload.
 */
const CREATE_MUTATION_FIELD_BY_TYPE: Record<string, CreatedEntity['type']> = {
    createProject: 'Project',
    createDeliverable: 'Deliverable',
    createAgentTask: 'AgentTask',
    createLargeLanguageModel: 'LargeLanguageModel',
};

/**
 * Tracks entities created during a Playwright test and deletes them by id
 * afterwards, so a run does not depend on the `[DeleteAfterTest]` marker.
 */
export class TestDataRegistry {
    private static entities: CreatedEntity[] = [];
    private static apiUrl: string;
    private static interceptionSetup = false;

    constructor() {
        TestDataRegistry.apiUrl = process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';
    }

    /** Adds an entity to the pending-cleanup list. */
    static registerEntity(type: CreatedEntity['type'], id: string): void {
        this.entities.push({ type, id });
    }

    /** Returns a copy of the pending-cleanup list. */
    static getEntities(): CreatedEntity[] {
        return [...this.entities];
    }

    /** Forgets all pending entities without deleting them. */
    static clear(): void {
        this.entities = [];
    }

    /** Starts observing create mutations on the given page (only the first call wins). */
    static setPage(page: Page): void {
        if (!TestDataRegistry.interceptionSetup && page) {
            TestDataRegistry.setupInterception(page);
            TestDataRegistry.interceptionSetup = true;
        }
    }

    /** Deletes every registered entity, newest first, and empties the registry. */
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

    /** Sends the `delete<Entity>(id: UUID!)` mutation for a single entity. */
    private async deleteEntity(entity: CreatedEntity): Promise<void> {
        const field = DELETE_FIELD_BY_TYPE[entity.type];
        if (!field) {
            return;
        }

        try {
            const errors = await deleteEntityById(TestDataRegistry.apiUrl, field, entity.id);

            if (errors.length > 0) {
                console.warn(`GraphQL errors deleting ${entity.type} ${entity.id}:`, errors);
            }
        } catch {
            // Ignore cleanup failures - entity may already be deleted
        }
    }

    /** Registers entities returned by create mutations observed on the page. */
    private static setupInterception(page: Page): void {
        page.on('response', async (response) => {
            const url = response.url();
            if (!url.includes('/graphql')) {
                return;
            }

            try {
                const json = await response.json();
                const data = json.data as Record<string, { id?: string } | null> | undefined;

                if (!data) {
                    return;
                }

                for (const [mutationName, type] of Object.entries(
                    CREATE_MUTATION_FIELD_BY_TYPE
                )) {
                    const id = data[mutationName]?.id;
                    if (id) {
                        TestDataRegistry.entities.push({ type, id });
                        console.log(`Registered ${type} for cleanup: ${id}`);
                    }
                }
            } catch {
                // Ignore parsing errors
            }
        });
    }
}
