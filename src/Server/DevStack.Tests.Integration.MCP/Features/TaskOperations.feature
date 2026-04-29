@devstack-tools
Feature: Task CRUD and Status Transition Operations
    Verify MCP server devstack tools for agent task management

    Scenario: Create a new agent task
        Given the MCP client is connected
        And a valid agent task creation request with title "Test Task"
        When I call create_task
        Then the response should contain the created task
        And the task should have a valid ID
        And the task status should be "Ready"

    Scenario: Update a task
        Given the MCP client is connected
        And an existing task ID
        When I call update_task with updated description "Updated Description"
        Then the response should contain the updated task

    Scenario: Transition task status
        Given the MCP client is connected
        And an existing task ID
        And a task in "Ready" status
        When I call update_task_state to "InProgress"
        Then the response should contain the task with new status
        And the status should be "InProgress"
