# DevStack Specification

# Goals

Create a framework to drive coding agents for continuous execution to make full use of local LLMs

# Components

- Graphql server holding the data model
- Admin UI for a user to configure projects and add features, monitor execution
- A driver component to plan and execute tasks

# Admin UI
- All fields are viewable
- Drop down for status, enable user to make state changes
- Edit project, Feature, Task
- Dashboard on projects in flight and items in NeedsReview or Failed
- Use react, tailwind, shadcn/ui

# Agent process
- Use typescript and langchain
- generate skills for git, pull request, update feature, create and update task
- Workflows
  - Planner: Execute planning for all Features that have status "Ready". Outcome: Tasks created that can be executed by an AI coding agent in less than 20 minutes, all task fields provided. Move to "InProgress" if there are no open questions or decisions. Move to "InReview" if there are open questions. Move to "Failed" if the agent failed to execute
  - DevLead. Once a feature is in "Ready" state, moves to "Prepare" and creates a feature branch. Then moves to "Code" status Once all tasks are finished, moves to "Review" and creates a pull request. Once PR is approved, Moves status to "Done". If there are PR comments, creates tasks to address them and moves back to "Code".
  - Coder: Execute Tasks for projects in "Code" state. Commit changes if quality gates pass. Move tasks to Complete. Move failed tasks to "Failed"
  - Tester: Run tests and review results. Create defects where fixes or improvements are needed
  - Architect: Operates on the project level. Reviews compliance with project coding standards. Creates Features for planned improvements. Keep documentation and knowledge current. Review for security concerns.

Include the opencode npm package in the agent process.
Mount a repositories volume into the agent process to hold any enlistments

# Non goals
- No authentication

# Project structure

Code and tests under src/


