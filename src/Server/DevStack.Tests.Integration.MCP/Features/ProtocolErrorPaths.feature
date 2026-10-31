@protocol-errors
Feature: Protocol Error Paths
    Verify how the MCP server answers malformed tools/call requests:
    unknown tool names, missing required arguments, wrong argument types and invalid enum literals

    Scenario: Calling a tool that does not exist
        Given the MCP client is connected
        When I call the unknown tool "get_all_the_things"
        Then the server rejects the call as a protocol error containing "Unknown tool"

    Scenario: Calling get_project without the required id argument
        Given the MCP client is connected
        When I call get_project with no arguments
        Then the server rejects the call as a tool execution error

    Scenario: Calling get_project with an id that is not a GUID
        Given the MCP client is connected
        When I call get_project with the id "not-a-guid"
        Then the server rejects the call as a tool execution error

    Scenario: Calling get_task with an id that is not a GUID
        Given the MCP client is connected
        When I call get_task with the id "12345"
        Then the server rejects the call as a tool execution error

    Scenario: Calling update_deliverable_status with an unknown deliverable status
        Given the MCP client is connected
        When I call update_deliverable_status with the target status "NOT_A_STATUS"
        Then the server rejects the call as a tool execution error

    Scenario: Calling get_next_deliverable with an unknown deliverable status
        Given the MCP client is connected
        When I call get_next_deliverable with the status "SOMETHING_ELSE"
        Then the server rejects the call as a tool execution error

    Scenario: Calling update_task_status with an unknown task status
        Given the MCP client is connected
        When I call update_task_status with the target status "NOPE"
        Then the server rejects the call as a tool execution error
