@devstack-tools
Feature: Task CRUD and Status Transition Operations
    Verify MCP server devstack tools for agent task management

    Scenario: Create a new agent task
        Given a valid agent task creation request with title "Test Task"
        When I call devstack_createAgentTask
        Then the response should contain the created task
        And the task should have a valid ID
        And the task status should be "Ready"

    Scenario: Update a task
        Given an existing task ID
        When I call devstack_updateAgentTask with updated title "Updated Task"
        Then the response should contain the updated task
        And the task title should be "Updated Task"

    Scenario: Transition task status
        Given a task in "ready" status
        When I call devstack_transitionAgentTaskStatus to "in_progress"
        Then the response should contain the task with new status
        And the status should be "InProgress"
