@devstack-tools
Feature: Dashboard Summary Operation
    Verify MCP server devstack_getDashboardSummary method

    Scenario: Get dashboard summary
        Given existing projects, features, tasks, and defects in the system
        When I call devstack_getDashboardSummary
        Then the response should contain total project count
        And the response should contain features in review count
        And the response should contain features failed count
        And the response should contain tasks in progress count
        And the response should contain tasks failed count
