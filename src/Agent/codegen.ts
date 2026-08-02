import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  overwrite: true,
  schema: process.env.GRAPHQL_ENDPOINT || 'http://localhost:8087/graphql',
  documents: ['src/graphql/**/*.graphql'],
  generates: {
    './src/gql/generated.ts': {
      plugins: [
        {
          add: {
            content: '// @ts-nocheck\n/* eslint-disable */'
          }
        },
        'typescript',
        'typescript-operations',
        'typescript-graphql-request'
      ],
      config: {
        rawRequest: false,
        enumsAsTypes: true,
        defaultScalarType: 'unknown',
        scalars: {
          UUID: 'string',
          Date: 'string',
          DateTime: 'string'
        }
      }
    },
  },
};

export default config;
