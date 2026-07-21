@tools-list
Feature: Tools List
    Verify MCP server tools/list returns available tools

    Scenario: List all available tools
        Given the MCP client is connected
        When I request the tool list
        Then the response should contain a list of tools
        And the tools should include "get_projects"
        And the tools should include "get_project"
        And the tools should include "create_project"
        And the tools should include "create_deliverable"
        And the tools should include "update_deliverable"
        And the tools should include "update_deliverable_status"
        And the tools should include "create_task"
        And the tools should include "update_task"
        And the tools should include "update_task_status"

    Scenario: Tool schema is properly defined
        Given the MCP client is connected
        When I request the tool list
        Then each tool should have a name
        And each tool should have a description
        And each tool should have inputSchema
