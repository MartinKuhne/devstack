import { test as base } from '@playwright/test';
import { TestDataRegistry } from './TestDataRegistry.js';

/* eslint-disable react-hooks/rules-of-hooks */
export const test = base.extend<{ _registry: TestDataRegistry }>({
    _registry: async ({ page }, useFn) => {
        const registry = new TestDataRegistry();
        TestDataRegistry.setPage(page);

        await useFn(registry as never);

        await registry.cleanup();
        TestDataRegistry.clear();
    },
});
/* eslint-enable react-hooks/rules-of-hooks */

export { expect } from '@playwright/test';
