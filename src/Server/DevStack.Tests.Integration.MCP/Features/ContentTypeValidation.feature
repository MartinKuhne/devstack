@error-handling
Feature: Content-Type Validation
    Verify HTTP Content-Type header handling with JSON-RPC 2.0 compliance

    Scenario: Send request with correct Content-Type
        Given a valid JSON-RPC request
        When I send the request with Content-Type "application/json"
        Then the response should be successful
        And the response should have jsonrpc field "2.0"
        And the response should have a result field
        And the response should not have an error field

    Scenario: Send request with wrong Content-Type
        Given a valid JSON-RPC request
        When I send the request with Content-Type "text/plain"
        Then the response should contain an error
        And the status code should indicate client error

    Scenario: Send request without Content-Type
        Given a valid JSON-RPC request
        When I send the request without Content-Type header
        Then the response should contain an error
