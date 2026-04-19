# Admin UI Specification

# Goals

The Admin UI supports a human software engineer, software engineering manager or product owner to give input to and manage the software development automation process

# Components

- A docker container hosting a web interface
- Integration tests for the admin UI (prefer gherkin and playwright)
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures

Prefer react, tailwind, shadcn/ui

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
 │   ├─ NavItem: Dashboard
 │   ├─ NavItem: Projects
 │   └─ NavItem: LargeLanguageModels
 └─ Project
     ├─ Deliverables
        ├─ AgentTasks

# Maintenance tasks
- Verify UI visual styles are correct
- Ensure use of ShadCN matches best practices

# Technical specification

- Log operations and errors to the console
- [ShadCN](https://ui.shadcn.com/docs)
- [ShadCN Theming](https://ui.shadcn.com/docs/theming)
- [ShadCN Components](https://ui.shadcn.com/docs/components)
