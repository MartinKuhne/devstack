@error-handling
Feature: Empty Body Handling
    Verify proper error handling for empty request bodies with JSON-RPC 2.0 compliance

    Scenario: Send empty body
        Given an empty request body
        When I send the POST request
        Then the response should contain error code -32700
        And the error message should indicate parse error
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Send whitespace only body
        Given a request body with only whitespace
        When I send the POST request
        Then the response should contain error code -32700
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data

    Scenario: Send null body
        Given a request body with literal "null"
        When I send the POST request
        Then the response should contain error code -32700
        And the response should have jsonrpc field "2.0"
        And the response should have an error object with code, message, and optional data
