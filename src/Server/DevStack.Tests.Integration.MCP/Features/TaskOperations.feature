@devstack-tools
Feature: Task CRUD and Status Transition Operations
    Verify MCP server devstack tools for task management

    Scenario: Create a new task
        Given a valid task creation request with title "Test Task"
        When I call devstack_createTask
        Then the response should contain the created task
        And the task should have a valid ID

    Scenario: Get task by ID
        Given an existing task ID
        When I call devstack_getTaskById with the ID
        Then the response should contain the task details

    Scenario: Get tasks with filters
        Given existing tasks in the system
        When I call devstack_getTasks with featureId filter
        Then the response should contain filtered tasks
        And all tasks should belong to the specified feature

    Scenario: Update a task
        Given an existing task ID
        When I call devstack_updateTask with updated title "Updated Task"
        Then the response should contain the updated task
        And the task title should be "Updated Task"

    Scenario: Transition task status
        Given a task in "todo" status
        When I call devstack_transitionTaskStatus to "in_progress"
        Then the response should contain the task with new status
        And the status should be "in_progress"

    Scenario: Delete a task
        Given an existing task ID
        When I call devstack_deleteTask with the ID
        Then the response should confirm deletion
