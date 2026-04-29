@devstack-tools
Feature: Task CRUD and Status Transition Operations
    Verify MCP server devstack tools for agent task management with JSON-RPC 2.0 compliance

    Scenario: Create a new agent task
        Given a valid agent task creation request with title "Test Task"
        When I call create_task
        Then the response should contain the created task
        And the task should have a valid ID
        And the task status should be "Ready"
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field

    Scenario: Update a task
        Given an existing task ID
        When I call update_task with updated description "Updated Description"
        Then the response should contain the updated task
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field

    Scenario: Transition task status
        Given an existing task ID
        And a task in "ready" status
        When I call update_task_state to "in_progress"
        Then the response should contain the task with new status
        And the status should be "InProgress"
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field
