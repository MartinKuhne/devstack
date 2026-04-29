@not-implemented
Feature: Not Implemented Methods
    Verify MCP server returns Method not found for unimplemented methods with JSON-RPC 2.0 compliance

    Scenario: Call resources/read method
        Given a resources/read request
        When I send the unimplemented request
        Then the response should contain error code -32601
        And the error message should contain "Method not found"
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Call prompts/list method
        Given a prompts/list request
        When I send the unimplemented request
        Then the response should contain error code -32601
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Call prompts/get method
        Given a prompts/get request
        When I send the unimplemented request
        Then the response should contain error code -32601
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Call completion/complete method
        Given a completion/complete request
        When I send the unimplemented request
        Then the response should contain error code -32601
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data
