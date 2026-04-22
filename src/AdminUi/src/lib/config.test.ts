import { describe, expect, it, vi } from 'vitest';

describe('config', () => {
    const originalEnv = { ...import.meta.env };

    afterEach(() => {
        Object.assign(import.meta.env, originalEnv);
    });

    it('uses VITE_API_URL when set', async () => {
        vi.resetModules();
        import.meta.env.VITE_API_URL = 'http://custom:8080/graphql';
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://custom:8080/graphql');
    });

    it('falls back to VITE_GRAPHQL_API_URL when VITE_API_URL is not set', async () => {
        vi.resetModules();
        delete import.meta.env.VITE_API_URL;
        import.meta.env.VITE_GRAPHQL_API_URL = 'http://fallback:9000/graphql';
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://fallback:9000/graphql');
    });

    it('falls back to default URL when neither env var is set', async () => {
        vi.resetModules();
        delete import.meta.env.VITE_API_URL;
        delete import.meta.env.VITE_GRAPHQL_API_URL;
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://localhost:5000/graphql');
    });

    it('prefers VITE_API_URL over VITE_GRAPHQL_API_URL', async () => {
        vi.resetModules();
        import.meta.env.VITE_API_URL = 'http://primary:3000/graphql';
        import.meta.env.VITE_GRAPHQL_API_URL = 'http://secondary:4000/graphql';
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://primary:3000/graphql');
    });
});
