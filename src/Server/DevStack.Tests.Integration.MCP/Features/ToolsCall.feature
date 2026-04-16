@tools-call
Feature: Tools Call Method
    Verify MCP server tools/call method invokes tools correctly

    Scenario: Call a valid tool with parameters
        Given a valid tools/call request for "devstack_getDashboardSummary"
        When I send the tools/call request
        Then the response should contain the tool result
        And the result should contain project count
        And the result should contain feature count

    Scenario: Call a tool with missing required parameters
        Given a tools/call request with missing required parameters
        When I send the tools/call request
        Then the response should contain an error
        And the error code should be -32602
