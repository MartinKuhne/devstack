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

# Components

- A docker container hosting a web interface
- Integration tests for the admin UI (run with ```npm run test:e2e```)
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures

# EARS (Easy Approach to Requirements Syntax) formatted universal functional requirements

- [REQ-UI-001] The system shall provide a graphical user interface to Create a LargeLanguageModel
- [REQ-UI-002] The system shall provide a graphical user interface to Edit a LargeLanguageModel
- [REQ-UI-003] The system shall provide a graphical user interface to Delete a LargeLanguageModel
- [REQ-UI-004] The system shall provide a graphical user interface to Create a Project
- [REQ-UI-005] The system shall provide a graphical user interface to Edit a Project
- [REQ-UI-006] The system shall provide a graphical user interface to Delete a Project
- [REQ-UI-007] The system shall provide a graphical user interface to Create a Deliverable
- [REQ-UI-008] The system shall provide a graphical user interface to Edit a Deliverable
- [REQ-UI-009] The system shall provide a graphical user interface to Change the Status of a Deliverable to any of the status values that are defined by the GraphQL schema
- [REQ-UI-010] The system shall provide a graphical user interface to Delete a Deliverable
- [REQ-UI-011] The system shall provide a graphical user interface to Create an AgentTask
- [REQ-UI-012] The system shall provide a graphical user interface to Edit an AgentTask
- [REQ-UI-013] The system shall provide a graphical user interface to Delete an AgentTask
- [REQ-UI-014] The system shall provide a graphical user interface to Change the Status of an AgentTask to any of the status values that are defined by the GraphQL schema
- [REQ-UI-015] The system shall provide a graphical user interface to View a dashboard showing the Count of Deliverables per Status each
- [REQ-UI-016] When the user has provided valid input to create an object, the system should [enable the Create button]
- [REQ-UI-017] When the user input is invalid, the system shall [display validation errors] and [disable the Create button].
- [REQ-UI-108] The system shall display the [Count of Deliverables] per [Status] using a [Table].

- [REQ-UI-200] The system shall allow the user to search for [Deliverables] by [Title] using the [SearchBar].
- [REQ-UI-201] When the user selects a [Project] from the [Sidebar], the system shall display the [Deliverables] list for that [Project].


- [REQ-UI-100] The system shall have unit tests
- [REQ-UI-101] The system shall have integration tests that excerise the live instance of the project
- [REQ-UI-102] When the system creates test data, it shall mark it as such by using the "[DeleteAfterTest]" text in the Title or Name of the object created
- [REQ-UI-103] When the system is beginning to run integration tests, it shall delete the test data
- [REQ-UI-104] When the system has finished running integration tests, it shall delete the test data

Include [Global non-functional requirements](../NON-FUNCTIONAL.md)

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

# Additional quality gates
- ```npm run build```
- ```npm run test```
- ```npm rum test:e2e```
