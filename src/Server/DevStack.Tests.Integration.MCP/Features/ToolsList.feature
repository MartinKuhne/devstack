@tools-list
Feature: Tools List Method
    Verify MCP server tools/list method returns available tools

    Scenario: List all available tools
        Given a valid tools/list request
        When I send the tools/list request
        Then the response should contain a list of tools
        And the tools should include "devstack_getProjects"
        And the tools should include "devstack_getProjectById"
        And the tools should include "devstack_createDeliverable"
        And the tools should include "devstack_updateDeliverable"
        And the tools should include "devstack_transitionDeliverableStatus"
        And the tools should include "devstack_createAgentTask"
        And the tools should include "devstack_updateAgentTask"
        And the tools should include "devstack_transitionAgentTaskStatus"

    Scenario: Tool schema is properly defined
        Given a valid tools/list request
        When I send the tools/list request
        Then each tool should have a name
        And each tool should have a description
        And each tool should have inputSchema
