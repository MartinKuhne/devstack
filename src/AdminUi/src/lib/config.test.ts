import { describe, expect, it, vi, afterEach } from 'vitest';

describe('config', () => {
    const originalEnv = { ...import.meta.env };

    afterEach(() => {
        Object.assign(import.meta.env, originalEnv);
        delete window.__ENV__;
    });

    it('prefers window.__ENV__.GRAPHQL_API_URL when set', async () => {
        vi.resetModules();
        window.__ENV__ = { GRAPHQL_API_URL: 'http://runtime:8080/graphql' };
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://runtime:8080/graphql');
    });

    it('uses VITE_API_URL when set and window.__ENV__ is absent', async () => {
        vi.resetModules();
        delete window.__ENV__;
        import.meta.env.VITE_API_URL = 'http://custom:8080/graphql';
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://custom:8080/graphql');
    });

    it('falls back to VITE_GRAPHQL_API_URL when VITE_API_URL is not set', async () => {
        vi.resetModules();
        delete window.__ENV__;
        delete import.meta.env.VITE_API_URL;
        import.meta.env.VITE_GRAPHQL_API_URL = 'http://fallback:9000/graphql';
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://fallback:9000/graphql');
    });

    it('falls back to default URL when no env var is set', async () => {
        vi.resetModules();
        delete window.__ENV__;
        delete import.meta.env.VITE_API_URL;
        delete import.meta.env.VITE_GRAPHQL_API_URL;
        const config = await import('./config');
        expect(config.default.GRAPHQL_API_URL).toBe('http://localhost:8087/graphql');
    });
});
