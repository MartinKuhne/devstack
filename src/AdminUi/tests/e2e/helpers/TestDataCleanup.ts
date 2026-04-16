import { TestDataRegistry } from './TestDataRegistry.js';

export async function cleanupTestData(): Promise<void> {
    const registry = new TestDataRegistry();
    await registry.cleanup();
    TestDataRegistry.clear();
}

export function registerEntity(type: 'Project' | 'Feature' | 'Defect' | 'Task' | 'Epic' | 'ModelConfiguration', id: string): void {
    TestDataRegistry.registerEntity(type, id);
}
