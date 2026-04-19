@not-implemented
Feature: Not Implemented Methods
    Verify MCP server returns Method not found for unimplemented methods

    Scenario: Call resources/read method
        Given a resources/read request
        When I send the unimplemented request
        Then the response should contain error code -32601
        And the error message should contain "Method not found"

    Scenario: Call prompts/list method
        Given a prompts/list request
        When I send the unimplemented request
        Then the response should contain error code -32601

    Scenario: Call prompts/get method
        Given a prompts/get request
        When I send the unimplemented request
        Then the response should contain error code -32601

    Scenario: Call completion/complete method
        Given a completion/complete request
        When I send the unimplemented request
        Then the response should contain error code -32601
