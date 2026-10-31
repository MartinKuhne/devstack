@tools-list
Feature: Tools List
    Verify MCP server tools/list returns available tools

    Scenario: List all available tools
        Given the MCP client is connected
        When I request the tool list
        Then the response should contain a list of tools
        And the tools should be exactly the expected tool set
        And the tools should include "get_projects"
        And the tools should include "get_project"
        And the tools should include "create_project"
        And the tools should include "get_deliverable"
        And the tools should include "get_next_deliverable"
        And the tools should include "create_deliverable"
        And the tools should include "update_deliverable"
        And the tools should include "update_deliverable_status"
        And the tools should include "get_task"
        And the tools should include "get_next_task"
        And the tools should include "create_task"
        And the tools should include "update_task"
        And the tools should include "update_task_status"

    Scenario: Tool schema is properly defined
        Given the MCP client is connected
        When I request the tool list
        Then each tool should have a name
        And each tool should have a description
        And each tool should have inputSchema
        And the tool "get_projects" should not require any parameters
        And the tool "get_project" should require the parameters id
        And the tool "create_project" should require the parameters name, repository
        And the tool "get_deliverable" should require the parameters id
        And the tool "get_next_deliverable" should require the parameters projectId, status
        And the tool "create_deliverable" should require the parameters projectId, title
        And the tool "update_deliverable" should require the parameters id
        And the tool "update_deliverable_status" should require the parameters id, targetStatus, actor
        And the tool "get_task" should require the parameters id
        And the tool "get_next_task" should not require any parameters
        And the tool "create_task" should require the parameters deliverableId, title
        And the tool "update_task" should require the parameters id
        And the tool "update_task_status" should require the parameters id, targetStatus, actor
