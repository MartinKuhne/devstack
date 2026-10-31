@tool-coverage
Feature: Tool Coverage Happy Paths
    Verify the MCP tools that previously had no integration coverage:
    get_project, create_project, get_deliverable, get_next_deliverable, get_task and get_next_task

    Scenario: get_project returns the seeded project
        Given the MCP client is connected
        When I call get_project for the seeded project
        Then the returned project id is the seeded project
        And the returned project name contains "MCP Test Project"

    Scenario: create_project stores and returns the new project
        Given the MCP client is connected
        When I create a coverage project named "Created"
        Then the created coverage project can be read back with get_project

    Scenario: get_deliverable returns every field that was created
        Given the MCP client is connected
        And a coverage project
        When I create a fully documented deliverable
        Then get_deliverable returns the documented deliverable fields

    Scenario: get_next_deliverable finds a deliverable in a known status
        Given the MCP client is connected
        And a coverage project
        And a coverage deliverable
        Then get_next_deliverable finds the coverage deliverable in "Draft" status
        When I move the coverage deliverable to "Design" status
        Then get_next_deliverable finds the coverage deliverable in "Design" status

    Scenario: get_task returns the created task
        Given the MCP client is connected
        And a coverage project
        And a coverage deliverable
        And a coverage task
        Then get_task returns the coverage task

    Scenario: get_next_task returns the ready task of an implement deliverable
        Given the MCP client is connected
        And a coverage project
        And a coverage deliverable in "Implement" status
        And a coverage task
        Then get_next_task returns the coverage task

    Scenario: get_next_task reports that all tasks of the deliverable are complete
        Given the MCP client is connected
        And a coverage project
        And a coverage deliverable in "Implement" status
        And a coverage task
        And the coverage task is moved to "Done" status
        Then get_next_task reports all tasks are completed
