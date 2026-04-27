import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
    schema: [
        'src/../../Server/DevStack.Api/GraphQL/schema.graphql',
    ],
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
                avoidOptionals: false,
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
