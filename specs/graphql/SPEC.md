# Graphql Specification

# Goals

Create a data model and API to manage an AI driven development process

# Components

- Graphql server holding the data model

# Graphql server

A graphql server on .net 10, hot chocolate, postgres, containerized
Integration tests are required for graphql to exercise all known mutations and their corner cases

## Data model

- Project
  - Name
  - Description
  - Repository
- Deliverable / 1:n relation to Project
  - Title
  - Status (Draft, Planning, Ready, InProgress, Done, Failed, Rejected, NeedsReview)
  - Type (Feature, Defect, Maintenance)
  - Description
  - Acceptance criteria (optional)
  - AgentFeedback (optional)
  - ExecutionPlan (optional)
  - Security impact (optional)
  - Performance impact (optional)
  - TestPlan (optional)
  - DeploymentPlan (optional)
  - Blocking (optional)
- AgentTask / 1:n relation to Deliverable
  - Title
  - Status (Ready, InProgress, Done, Failed, Rejected, NeedsReview)
  - DeliverableId
  - Result (optional)
  - Errors (optional)
  - CommitHash (optional)
  - Complexity rating (1 to 10)
  - DependsOnDevTask (optional)
  - PromptTokens (optional)
  - CompletionTokens (optional)
  - ExecutionDurationInSeconds (optional)
  - Model (the model that executed the task) (optional)
- LargeLanguageModel / 1:n relation to Project
  - Url
  - Model
  - ModelAlias
  - API key
  - MaxComplexity
  - MaxConcurrency

# Non goals
- No authentication

# Project structure

Code and tests under src/


