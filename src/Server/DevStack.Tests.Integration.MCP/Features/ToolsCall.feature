@tools-call
Feature: Tools Call Method
    Verify MCP server tools/call method invokes tools correctly with JSON-RPC 2.0 compliance

    Scenario: Call a valid tool with parameters
        Given a valid tools/call request for "get_projects"
        When I send the tools/call request
        Then the response should contain the tool result
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field
        And the result should contain a content array

    Scenario: Call a tool with missing required parameters
        Given a tools/call request with missing required parameters
        When I send the tools/call request
        Then the response should contain an error with code -32602
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have an error field
        And the response should not have a result field
