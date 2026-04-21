import { TestDataRegistry } from './TestDataRegistry.js';

export async function cleanupTestData(): Promise<void> {
    const registry = new TestDataRegistry();
    await registry.cleanup();
    TestDataRegistry.clear();
}

export function registerEntity(type: 'Project' | 'Deliverable' | 'AgentTask' | 'LargeLanguageModel', id: string): void {
    TestDataRegistry.registerEntity(type, id);
}
