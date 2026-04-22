interface TestEntity {
    id: string;
    type: 'project' | 'feature' | 'task' | 'defect' | 'modelConfiguration';
}

export class TestDataManager {
    private createdEntities: TestEntity[] = [];
    private apiUrl: string;

    constructor(apiUrl: string) {
        this.apiUrl = apiUrl;
    }

    async createProject(name: string, url?: string): Promise<{ id: string }> {
        const mutation = `
            mutation CreateProject($name: String!, $url: String) {
                createProject(input: { name: $name, url: $url }) {
                    project {
                        id
                    }
                }
            }
        `;

        const response = await fetch(this.apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: mutation,
                variables: { name, url },
            }),
        });

        const data = (await response.json()) as {
            data: { createProject: { project: { id: string } } };
        };
        const projectId = data.data.createProject.project.id;

        this.createdEntities.push({ id: projectId, type: 'project' });
        return { id: projectId };
    }

    async createFeature(projectId: string, title: string): Promise<{ id: string }> {
        const mutation = `
            mutation CreateFeature($projectId: ID!, $title: String!) {
                createFeature(input: { projectId: $projectId, title: $title }) {
                    feature {
                        id
                    }
                }
            }
        `;

        const response = await fetch(this.apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: mutation,
                variables: { projectId, title },
            }),
        });

        const data = (await response.json()) as {
            data: { createFeature: { feature: { id: string } } };
        };
        const featureId = data.data.createFeature.feature.id;

        this.createdEntities.push({ id: featureId, type: 'feature' });
        return { id: featureId };
    }

    async createTask(featureId: string, title: string): Promise<{ id: string }> {
        const mutation = `
            mutation CreateTask($featureId: ID!, $title: String!) {
                createTask(input: { featureId: $featureId, title: $title }) {
                    task {
                        id
                    }
                }
            }
        `;

        const response = await fetch(this.apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: mutation,
                variables: { featureId, title },
            }),
        });

        const data = (await response.json()) as { data: { createTask: { task: { id: string } } } };
        const taskId = data.data.createTask.task.id;

        this.createdEntities.push({ id: taskId, type: 'task' });
        return { id: taskId };
    }

    async createDefect(
        featureId: string,
        title: string,
        severity: string
    ): Promise<{ id: string }> {
        const mutation = `
            mutation CreateDefect($featureId: ID!, $title: String!, $severity: Severity!) {
                createDefect(input: { featureId: $featureId, title: $title, severity: $severity }) {
                    defect {
                        id
                    }
                }
            }
        `;

        const response = await fetch(this.apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: mutation,
                variables: { featureId, title, severity },
            }),
        });

        const data = (await response.json()) as {
            data: { createDefect: { defect: { id: string } } };
        };
        const defectId = data.data.createDefect.defect.id;

        this.createdEntities.push({ id: defectId, type: 'defect' });
        return { id: defectId };
    }

    async cleanup(): Promise<void> {
        const deleteMutation = `
            mutation DeleteEntity($id: ID!, $type: EntityType!) {
                deleteEntity(id: $id, type: $type)
            }
        `;

        for (const entity of this.createdEntities.reverse()) {
            try {
                const entityType =
                    entity.type === 'project'
                        ? 'PROJECT'
                        : entity.type === 'feature'
                          ? 'FEATURE'
                          : entity.type === 'task'
                            ? 'TASK'
                            : entity.type === 'defect'
                              ? 'DEFECT'
                              : 'MODEL_CONFIGURATION';

                await fetch(this.apiUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        query: deleteMutation,
                        variables: { id: entity.id, type: entityType },
                    }),
                });
            } catch (error) {
                console.warn(`Failed to delete ${entity.type} ${entity.id}:`, error);
            }
        }

        this.createdEntities = [];
    }

    getCreatedEntities(): TestEntity[] {
        return [...this.createdEntities];
    }
}
