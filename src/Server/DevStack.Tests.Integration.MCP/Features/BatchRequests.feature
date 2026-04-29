@error-handling
Feature: Batch Requests
    Verify JSON-RPC batch request handling with JSON-RPC 2.0 compliance

    Scenario: Send batch of valid requests
        Given an array of 3 valid JSON-RPC requests
        When I send the batch request
        Then the response should contain 3 responses
        And each response should have the correct id
        And each response should have jsonrpc field "2.0"

    Scenario: Send batch with mixed requests and notifications
        Given an array with 2 requests and 1 notification
        When I send the batch request
        Then the response should contain 2 responses (notifications excluded)
        And each response should have jsonrpc field "2.0"

    Scenario: Send batch with one invalid request
        Given an array with 2 valid requests and 1 invalid request
        When I send the batch request
        Then the response should contain responses for all requests
        And the invalid request response should contain error
        And each response should have jsonrpc field "2.0"
