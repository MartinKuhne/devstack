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

# GraphQL Specification

# Goals

Create a data model and API to manage an AI driven development process

# Components

- Graphql server holding the data model
- Unit tests
- Integration tests using Testcontainers for .NET (run with ```dotnet test src/server/```)

# Graphql server

- A graphql server on .net 10, hot chocolate, postgres, containerized

Include [Global non-functional requirements](../NON-FUNCTIONAL.md)

# EARS (Easy Approach to Requirements Syntax) formatted functional requirements

- [GRAPHQL-001] The system shall support mutations and queries to create, read, update and delete the entities in the [entity relationship diagram](data-model.mmd)
- [GRAPHQL-002] The system shall support optional filtering
- [GRAPHQL-003] The system shall support optional sorting
- [GRAPHQL-004] The system shall support optional paging
- [GRAPHQL-005] The system shall expose an HTTP endpoint at the path /graphql
- [GRAPHQL-006] The system shall expose a web site to enable the user to view the schema and perform queries and mutations.
- [GRAPHQL-007] The system shall provide a DeleteTestData mutation that deletes objects with the "[DeleteAfterTest]" text in the Title or Name of the object created

- [GRAPHQL-200] The system shall expose an HTTP GET endpoint at the path /health as a health check
- [GRAPHQL-201] The system shall return all health check responses in JSON format.
- [GRAPHQL-202] If the request targets the /health endpoint, the system shall not require authentication or authorization headers.
- [GRAPHQL-203] While the system and all its critical dependencies are operational, the system shall respond with the HTTP status code 200 OK.
- [GRAPHQL-204] When the system detects a critical failure (e.g., database connection loss), it shall respond with the HTTP status code 503 Service Unavailable.
- [GRAPHQL-205] When the health check is executed, the system shall attempt to open a connection to the primary database.

# Technical specification

- [GraphQL Specification](https://spec.graphql.org/September2025/)
- [Gherkin](https://cucumber.io/docs/gherkin/reference)
- [TestContainers](https://dotnet.testcontainers.org/)

# Non goals
- No authentication

