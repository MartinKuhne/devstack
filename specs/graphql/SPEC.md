<!--
FILE IS IMMUTABLE

This file is part of the system specification.
Automated agents MUST NOT:
- rewrite
- reformat
- optimize
- refactor
- regenerate
- insert or remove content

Only human maintainers may update this file.
-->

# Graphql Specification

# Goals

Create a data model and API to manage an AI driven development process

# Components

- Graphql server holding the data model
- Integration tests using Testcontainers for .NET (run with ```dotnet test src/server/```)

# Graphql server

- A graphql server on .net 10, hot chocolate, postgres, containerized
- Optional querying, sorting, filtering and paging
- Integration tests create queries and mutations to CRUD operations for all types in the data model

## Data model

I can create, read, update and delete the entities in the [entity relationship diagram](data-model.mmd)

# Technical specification

- Log operations and errors to the console
- [GraphQL Specification](https://spec.graphql.org/September2025/)
- [Gherkin](https://cucumber.io/docs/gherkin/reference)
- [TestContainers](https://dotnet.testcontainers.org/)

# Non goals
- No authentication

