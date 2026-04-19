# Graphql Specification

# Goals

Create a data model and API to manage an AI driven development process

# Components

- Graphql server holding the data model
- Integration tests using Testcontainers for .NET

# Graphql server

A graphql server on .net 10, hot chocolate, postgres, containerized
Integration tests are required for graphql to exercise all known mutations and their corner cases

## Data model

See the [entity relationship diagram](data-model.mmd)

# Technical specification

Log operations and errors to the console

# Non goals
- No authentication

