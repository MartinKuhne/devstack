@error-handling
Feature: Parse Error
    Verify JSON-RPC -32700 ParseError for malformed JSON

    Scenario: Send malformed JSON
        Given a request with invalid JSON syntax
        When I send the request
        Then the response should contain error code -32700
        And the error message should contain "Parse error"

    Scenario: Send truncated JSON
        Given a request with truncated JSON body
        When I send the request
        Then the response should contain error code -32700
