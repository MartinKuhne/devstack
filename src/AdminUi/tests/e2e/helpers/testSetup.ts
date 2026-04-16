import { test as base } from '@playwright/test';
import { TestDataRegistry } from './TestDataRegistry.js';

export const test = base.extend<{ _registry: TestDataRegistry }>({
    _registry: async ({ page }, use) => {
        const registry = new TestDataRegistry();
        TestDataRegistry.setPage(page);
        
        await use(registry as never);
        
        await registry.cleanup();
        TestDataRegistry.clear();
    },
});

export { expect } from '@playwright/test';
