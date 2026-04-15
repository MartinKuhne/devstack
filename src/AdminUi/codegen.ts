import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
    schema: 'http://localhost:8087/graphql',
    documents: 'src/graphql/**/*.graphql',
    generates: {
        'src/generated/graphql.ts': {
            plugins: [
                { add: { content: '// @ts-nocheck' } },
                'typescript',
                'typescript-operations',
                'typescript-react-apollo',
            ],
            config: {
                avoidOptionals: true,
                namingConvention: 'keep',
                enumsAsConst: true,
                withHooks: true,
                withHOC: false,
                withComponent: false,
                apolloReactHooksImportFrom: '@apollo/client/react',
            },
        },
    },
    ignoreNoDocuments: true,
};

export default config;
