@error-handling
Feature: Invalid Params
    Verify JSON-RPC -32602 InvalidParams for missing or malformed parameters

    Scenario: Missing required parameter
        Given a tools/call request without required "name" parameter
        When I send the request
        Then the response should contain error code -32602
        And the error message should contain "Invalid params"

    Scenario: Invalid parameter type
        Given a tools/call request with wrong parameter type
        When I send the request
        Then the response should contain error code -32602

    Scenario: Extra unknown parameters
        Given a tools/call request with unknown parameters
        When I send the request
        Then the response should be accepted
        And unknown parameters should be ignored
