# Graphql Specification

# Goals

Create a data model and API to manage an AI driven development process

# Components

- Graphql server holding the data model
- Integration tests using Testcontainers for .NET (run with ```dotnet test src/server/```)

# Graphql server

- A graphql server on .net 10, hot chocolate, postgres, containerized
- Integration tests test CRUD operations for all types in the data model
  -   - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures
  -   
## Data model

I can create, read, update and delete the entities in the [entity relationship diagram](data-model.mmd)

# Technical specification

- Log operations and errors to the console
- [GraphQL Specification](https://spec.graphql.org/September2025/)
- [Gherkin](https://cucumber.io/docs/gherkin/reference)

# Non goals
- No authentication

