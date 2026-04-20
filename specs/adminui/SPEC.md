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

# Admin UI Specification

# Goals

The Admin UI supports a human software engineer, software engineering manager or product owner to give input to and manage the software development automation process

# Scope

The scope of this projec

# Components

- A docker container hosting a web interface
- Integration tests for the admin UI (run with ```npm run test:e2e```)
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures

# Capabilities

Uses the data model provided by the graphql component.

A supervisor can
- Create a LargeLanguageModel
- Edit a LargeLanguageModel
- Delete a LargeLanguageModel
- Create a Project
- Edit a Project
- Delete a Project
- Create a Deliverable
- Edit a Deliverable
- Change the Status of a Deliverable
- Delete a Deliverable
- Create an AgentTask
- Edit an AgentTash
- Delete an AgentTask
- Change the Status of an AgentTask
- View a dashboard
  - Count of Deliverables in Planning state
  - Count of Deliverables in Ready state
  - Count of Deliverables in InProgress state
  - Count of Deliverables in NeedsReview state

# Structure

App
 ├─ Header
 │   ├─ Logo
 │   ├─ SearchBar
 │   └─ UserMenu
 ├─ Sidebar
 │   ├─ Dropdown: Project (User selects project)
 │   ├─ NavItem: Dashboard
 │   ├─ NavItem: LargeLanguageModels
 │   ├─ NavItem: Projects
 │   ├─ NavItem: Deliverables (when a Project is selected)
 │   └─ NavItem: AgentTasks (when a Deliverable is selected)
 └─ Project
     ├─ Deliverables (List view)
        └─  Deliverable (Detail view)

Deliverable detail view

+--------------------------------------------------------+
| Title                                  | Type | Status |
+--------------------------------------------------------+
| Description (Text block)               | AgentTasks    |
| AceptanceCriteria (Text block)         |               |
| ExecutionPlan (Text block)             |               |
| SecurityImpact (Text block)            |               |
| PerformanceImpact (Text block)         |               | 
| TestPlan (Text block)                  |               |
| DeploymentPlan (Text block)            |               |
| AgentFeedback (Text block)             |               |
| Blocking (Text block)                  |               |
+--------------------------------------------------------+


# Maintenance tasks
- Verify UI visual styles are correct
- Ensure use of ShadCN matches best practices

# Technical specification

- [React](https://react.dev/reference/react)
- [Tailwind](https://tailwindcss.com/plus/ui-blocks/documentation/elements)
- [ShadCN](https://ui.shadcn.com/docs)
- [ShadCN Theming](https://ui.shadcn.com/docs/theming)
- [ShadCN Components](https://ui.shadcn.com/docs/components)
- [Playwright](https://playwright.dev/docs/writing-tests)
- [TestContainers](https://dotnet.testcontainers.org/)

# Preferred libraries
- react
- tailwind
- shadcn/ui
- gherkin
- playwright
