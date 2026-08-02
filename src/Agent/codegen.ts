import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  overwrite: true,
  schema: process.env.GRAPHQL_ENDPOINT || 'http://localhost:8087/graphql',
  documents: ['src/graphql/**/*.graphql'],
  generates: {
    // Schema types only (Scalars, object types, input types, enums, filter inputs).
    './src/gql/graphql.ts': {
      plugins: ['typescript'],
      config: {
        enumsAsTypes: true,
        defaultScalarType: 'unknown',
        scalars: {
          UUID: 'string',
          Date: 'string',
          DateTime: 'string'
        }
      }
    },
    // Operation result types + typed document constants. `importSchemaTypesFrom`
    // tells `typescript-operations` to import (not re-emit) schema types from
    // `./graphql.js`, which is what eliminates the duplicate `export type
    // DeliverableStatus` / `DeliverableType` declarations that the legacy
    // `typescript-graphql-request` chain produced. The `typed-document-node`
    // plugin then annotates each document constant with
    // `TypedDocumentNode<Query, Variables>`, so `client.request(GetProjectsDocument, { first })`
    // infers the response and variables types automatically.
    './src/gql/operations.ts': {
      plugins: ['typescript-operations', 'typed-document-node'],
      config: {
        importSchemaTypesFrom: './graphql.js',
        nonOptionalTypename: false,
        skipTypename: true,
        useTypeImports: true
      }
    }
  }
};

export default config;
