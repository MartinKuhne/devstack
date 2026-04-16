const config = {
    GRAPHQL_API_URL: import.meta.env.VITE_GRAPHQL_API_URL || 
                     import.meta.env.GRAPHQL_API_URL || 
                     'http://localhost:5000/graphql',
};

export default config;
