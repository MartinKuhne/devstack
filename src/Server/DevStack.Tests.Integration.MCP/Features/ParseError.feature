@error-handling
Feature: Parse Error
    Verify JSON-RPC -32700 ParseError for malformed JSON with JSON-RPC 2.0 compliance

    Scenario: Send malformed JSON
        Given a request with invalid JSON syntax
        When I send the request
        Then the response should contain error code -32700
        And the error message should contain "Parse error"
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Send truncated JSON
        Given a request with truncated JSON body
        When I send the request
        Then the response should contain error code -32700
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data
