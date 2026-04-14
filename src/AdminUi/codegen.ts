import type { CodegenConfig } from '@graphql-codegen/cli'

const config: CodegenConfig = {
  schema: ['src/graphql/schema.graphql', 'src/graphql/**/*.graphql'],
  documents: 'src/graphql/**/*.graphql',
  generates: {
    'src/generated/graphql.ts': {
      plugins: ['typescript', 'typescript-operations'],
      config: {
        avoidOptionals: true,
        namingConvention: 'keep',
        enumsAsConst: true
      }
    }
  },
  ignoreNoDocuments: true
}

export default config
