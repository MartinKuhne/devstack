import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
    schema: 'http://localhost:8087/graphql',
    documents: 'src/graphql/**/*.graphql',
    generates: {
        'src/generated/graphql.ts': {
            plugins: ['typescript', 'typescript-operations', 'typescript-react-apollo'],
            config: {
                avoidOptionals: true,
                namingConvention: 'keep',
                enumsAsConst: true,
                withHooks: true,
                withHOC: false,
                withComponent: false,
            },
        },
    },
    ignoreNoDocuments: true,
};

export default config;
