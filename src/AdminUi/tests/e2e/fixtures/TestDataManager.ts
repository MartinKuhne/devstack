import {
    deleteEntityById,
    postGraphQL,
    withTestDataMarker,
} from '../helpers/testDataOperations.js';

/** Entity kinds the manager can create and clean up. */
export type TestEntityKind = 'project' | 'deliverable' | 'agentTask' | 'largeLanguageModel';

/** A fixture row created by {@link TestDataManager}. */
export interface TestEntity {
    id: string;
    type: TestEntityKind;
}

/** Deliverable kinds accepted by `CreateDeliverableInput.type` (a String in the schema). */
export type TestDeliverableType = 'FEATURE' | 'DEFECT' | 'MAINTENANCE' | 'SPIKE';

/** Statuses accepted by `CreateDeliverableInput.initialStatus` (`DeliverableStatus` enum). */
export type TestDeliverableStatus =
    | 'DRAFT'
    | 'DESIGN'
    | 'PLAN'
    | 'IMPLEMENT'
    | 'MERGE'
    | 'DEPLOY'
    | 'TEST'
    | 'DONE'
    | 'FAILED'
    | 'REJECTED'
    | 'NEEDS_REVIEW';

/** Mutation field used to remove each kind of fixture row. */
const DELETE_FIELD_BY_KIND: Record<TestEntityKind, Parameters<typeof deleteEntityById>[1]> = {
    project: 'deleteProject',
    deliverable: 'deleteDeliverable',
    agentTask: 'deleteAgentTask',
    largeLanguageModel: 'deleteLargeLanguageModel',
};

/**
 * Creates GraphQL fixtures through the real API and removes them again.
 *
 * Every generated `Project.name`, `Deliverable.title`, `AgentTask.title` and
 * `LargeLanguageModel.url` is forced to contain the `[DeleteAfterTest]` marker, so
 * the server-side `deleteTestData` cleanup reclaims the rows even if `cleanup()`
 * is never called.
 */
export class TestDataManager {
    private createdEntities: TestEntity[] = [];
    private apiUrl: string;

    constructor(apiUrl: string) {
        this.apiUrl = apiUrl;
    }

    /**
     * Creates a project (`createProject(input: CreateProjectInput!)`).
     * `repository` is required by the schema, so a placeholder is used when omitted.
     */
    async createProject(
        name: string,
        repository = 'https://example.com/e2e/repository.git',
        description = 'E2E test project'
    ): Promise<{ id: string }> {
        const document = `
            mutation CreateProject($input: CreateProjectInput!) {
                createProject(input: $input) {
                    id
                }
            }
        `;

        const id = await this.mutateForId(
            'createProject',
            document,
            {
                input: {
                    name: withTestDataMarker(name),
                    repository,
                    description,
                },
            },
            'project'
        );

        return { id };
    }

    /**
     * Creates a deliverable (`createDeliverable(input: CreateDeliverableInput!)`).
     * Replaces the removed `createFeature` / `createDefect` mutations: the kind is now
     * the `type` string and the severity concept no longer exists.
     */
    async createDeliverable(
        projectId: string,
        title: string,
        type: TestDeliverableType = 'FEATURE',
        initialStatus: TestDeliverableStatus = 'DRAFT',
        description = 'E2E test deliverable'
    ): Promise<{ id: string }> {
        const document = `
            mutation CreateDeliverable($input: CreateDeliverableInput!) {
                createDeliverable(input: $input) {
                    id
                }
            }
        `;

        const id = await this.mutateForId(
            'createDeliverable',
            document,
            {
                input: {
                    projectId,
                    title: withTestDataMarker(title),
                    type,
                    description,
                    initialStatus,
                },
            },
            'deliverable'
        );

        return { id };
    }

    /**
     * Creates an agent task (`createAgentTask(input: CreateAgentTaskInput!)`).
     * Replaces the removed `createTask` mutation; the input needs both the
     * `deliverableId` and the owning `projectId`.
     */
    async createAgentTask(
        projectId: string,
        deliverableId: string,
        title: string,
        description = 'E2E test task',
        complexityRating = 5
    ): Promise<{ id: string }> {
        const document = `
            mutation CreateAgentTask($input: CreateAgentTaskInput!) {
                createAgentTask(input: $input) {
                    id
                }
            }
        `;

        const id = await this.mutateForId(
            'createAgentTask',
            document,
            {
                input: {
                    projectId,
                    deliverableId,
                    title: withTestDataMarker(title),
                    description,
                    complexityRating,
                },
            },
            'agentTask'
        );

        return { id };
    }

    /**
     * Creates a large language model configuration
     * (`createLargeLanguageModel(input: CreateLargeLanguageModelInput!)`).
     * The marker is written into the `url`, which is what the server matches on.
     */
    async createLargeLanguageModel(
        url = 'https://example.com/e2e/v1/chat/completions',
        model = 'e2e-model',
        modelAlias = 'e2e-alias',
        apiKey = 'e2e-api-key'
    ): Promise<{ id: string }> {
        const document = `
            mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) {
                createLargeLanguageModel(input: $input) {
                    id
                }
            }
        `;

        const id = await this.mutateForId(
            'createLargeLanguageModel',
            document,
            {
                input: {
                    url: withTestDataMarker(url),
                    model,
                    modelAlias,
                    apiKey,
                },
            },
            'largeLanguageModel'
        );

        return { id };
    }

    /** Deletes every fixture that was created, newest first. */
    async cleanup(): Promise<void> {
        const entities = this.createdEntities.slice().reverse();
        this.createdEntities = [];

        for (const entity of entities) {
            try {
                const errors = await deleteEntityById(
                    this.apiUrl,
                    DELETE_FIELD_BY_KIND[entity.type],
                    entity.id
                );

                if (errors.length > 0) {
                    console.warn(`Failed to delete ${entity.type} ${entity.id}:`, errors);
                }
            } catch (error) {
                console.warn(`Failed to delete ${entity.type} ${entity.id}:`, error);
            }
        }
    }

    /** Returns a copy of the fixtures created so far. */
    getCreatedEntities(): TestEntity[] {
        return [...this.createdEntities];
    }

    /**
     * Runs a create mutation and returns the id of the created entity.
     * Throws with the GraphQL error messages instead of failing on an undefined access.
     */
    private async mutateForId(
        field: string,
        document: string,
        variables: Record<string, unknown>,
        kind: TestEntityKind
    ): Promise<string> {
        const result = await postGraphQL<Record<string, { id?: string } | null>>(
            this.apiUrl,
            document,
            variables
        );

        if (result.errors && result.errors.length > 0) {
            throw new Error(
                `GraphQL errors while creating ${kind}: ${result.errors
                    .map((error) => error.message)
                    .join('; ')}`
            );
        }

        const id = result.data?.[field]?.id;
        if (!id) {
            throw new Error(`Creating ${kind} returned no id`);
        }

        this.createdEntities.push({ id, type: kind });
        return id;
    }
}
