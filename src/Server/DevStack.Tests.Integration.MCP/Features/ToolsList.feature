@tools-list
Feature: Tools List Method
    Verify MCP server tools/list method returns available tools with JSON-RPC 2.0 compliance

    Scenario: List all available tools
        Given a valid tools/list request
        When I send the tools/list request
        Then the response should contain a list of tools
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field
        And the tools should include "get_projects"
        And the tools should include "get_project"
        And the tools should include "create_deliverable"
        And the tools should include "update_deliverable"
        And the tools should include "update_deliverable_state"
        And the tools should include "create_task"
        And the tools should include "update_task"
        And the tools should include "update_task_state"

    Scenario: Tool schema is properly defined
        Given a valid tools/list request
        When I send the tools/list request
        Then each tool should have a name
        And each tool should have a description
        And each tool should have inputSchema
