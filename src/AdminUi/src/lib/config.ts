declare global {
    interface Window {
        __ENV__?: {
            GRAPHQL_API_URL?: string;
            VITE_GRAPHQL_API_URL?: string;
            VITE_API_URL?: string;
        };
    }
}

const config = {
    GRAPHQL_API_URL: window.__ENV__?.GRAPHQL_API_URL ||
                     window.__ENV__?.VITE_GRAPHQL_API_URL ||
                     window.__ENV__?.VITE_API_URL ||
                     import.meta.env.VITE_API_URL ||
                     import.meta.env.VITE_GRAPHQL_API_URL ||
                     'http://localhost:8087/graphql',
};

export default config;
