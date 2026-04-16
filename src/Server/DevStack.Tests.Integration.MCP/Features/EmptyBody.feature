@error-handling
Feature: Empty Body Handling
    Verify proper error handling for empty request bodies

    Scenario: Send empty body
        Given an empty request body
        When I send the POST request
        Then the response should contain error code -32700
        And the error message should indicate parse error

    Scenario: Send whitespace only body
        Given a request body with only whitespace
        When I send the POST request
        Then the response should contain error code -32700

    Scenario: Send null body
        Given a request body with literal "null"
        When I send the POST request
        Then the response should contain error code -32700
