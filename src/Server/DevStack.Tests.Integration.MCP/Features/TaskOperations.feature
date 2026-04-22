@devstack-tools
Feature: Task CRUD and Status Transition Operations
    Verify MCP server devstack tools for agent task management

    Scenario: Create a new agent task
        Given a valid agent task creation request with title "Test Task"
        When I call create_task
        Then the response should contain the created task
        And the task should have a valid ID
        And the task status should be "Ready"

    Scenario: Update a task
        Given an existing task ID
        When I call update_task with updated description "Updated Description"
        Then the response should contain the updated task

    Scenario: Transition task status
        Given an existing task ID
        And a task in "ready" status
        When I call update_task_state to "in_progress"
        Then the response should contain the task with new status
        And the status should be "InProgress"
