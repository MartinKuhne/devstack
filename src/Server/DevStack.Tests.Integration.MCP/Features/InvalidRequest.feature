@error-handling
Feature: Invalid Request
    Verify JSON-RPC -32600 InvalidRequest for invalid request structure

    Scenario: Missing jsonrpc version
        Given a request without jsonrpc version field
        When I send the request
        Then the response should contain error code -32600
        And the error message should contain "Invalid Request"

    Scenario: Invalid jsonrpc version
        Given a request with invalid jsonrpc version "1.0"
        When I send the request
        Then the response should contain error code -32600

    Scenario: Missing method field
        Given a request without method field
        When I send the request
        Then the response should contain error code -32600

    Scenario: Missing id field for request
        Given a request without id field (not a notification)
        When I send the request
        Then the response should contain error code -32600
