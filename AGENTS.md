# Agent Principles

# Scope
Work only on the item described in the prompt. Do not modify unrelated code or create unrelated commits.

# Development process
- Check the quality gats before beginnging work. If there are compilation errors or tests fail, repair the tests before beginning work (unless instrcutged to pause tests). You are allowed to disable tests that test functionality that is no longer a requirement. If substantial test changes were needed, commit the changes and exit.
- Create a detailed plan and decompose implementations steps into units of work that can be done by an AI agent in less than 20 minutes
- Specify dependencies, test impact, architecture changes, risk
- Specify complexity on a scale of 1 to 10
- Create todos
- Execute the plan
- Check that all quality gates have passed
- Create a commit message with a summary of changes and commit

## Quality Gates
All quality gates must pass before marking an item done and commiting the changes, unless instructed to pause testing
- Build succeeds
- Unit tests pass
- No new lint errors or warnings

## Libraries
- Prefer libraries over re-inventing the wheel
- Libraries must be under a permissive license and have no licensing cost
- Use the latest stable version applicable to the current framework version and other libraries which are already in use
- Always research the correct usage for the current version, using context7 or web search

## Commits
- One focused commit per work item
- Write a clear commit message describing what changed and why
- Do not commit unrelated changes

# Code quality
- Once class per file
- All public and internal methods have a brief description
- Follow SOLID principles
- Say it once
- Do not put Client IDs and any passwords and secrets in the code
- Use immutable data structures, pure and honest functions

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->

# Domain model

- ./src/Server/DevStack.Api/ and ./src/Server.DevStack.Domain/ PRODUCE ```src/schema.graphql```. Any changes in these projects MUST be reflected in ```src/schema.graphql```
- All other projects CONSUME ```src/schema.graphql``` and MUST NOT change it when changes are applied

