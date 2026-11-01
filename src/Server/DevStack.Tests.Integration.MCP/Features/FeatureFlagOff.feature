@feature-flag-off
Feature: Feature Flag Off Hides The Task Tools
    The API-hosted MCP endpoint registers TaskTools only when FeatureManagement:AgentTaskTools is true.
    A second, isolated API environment is booted with the flag switched off to prove that the
    five task tools disappear from tools/list and that create_task is no longer a known tool,
    while the project and deliverable tools keep working.

    Scenario: tools/list advertises only the project and deliverable tools
        Given the flag-off MCP client is connected
        When I request the flag-off tool list
        Then the flag-off tools are exactly the project and deliverable tools

    Scenario: every task tool is hidden from tools/list
        Given the flag-off MCP client is connected
        When I request the flag-off tool list
        Then the flag-off tools do not include "get_task"
        And the flag-off tools do not include "get_next_task"
        And the flag-off tools do not include "create_task"
        And the flag-off tools do not include "update_task"
        And the flag-off tools do not include "update_task_status"

    Scenario: create_task is answered as an unknown tool
        Given the flag-off MCP client is connected
        When I call the flag-off create_task tool
        Then the flag-off call fails with the protocol error "Unknown tool"

    Scenario: the database backed project tools keep working
        Given the flag-off MCP client is connected
        When I call the flag-off get_projects tool
        Then the flag-off call succeeds
