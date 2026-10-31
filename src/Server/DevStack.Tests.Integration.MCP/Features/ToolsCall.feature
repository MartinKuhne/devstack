@tools-call
Feature: Tools Call
    Verify MCP server tools/call method invokes tools correctly

    Scenario: Call a valid tool with parameters
        Given the MCP client is connected
        And a valid tools/call request for "get_projects"
        When I send the tools/call request
        Then the response should contain the tool result
        And the result should contain a content array

    Scenario: Call a tool with an unknown name
        Given the MCP client is connected
        And a tools/call request for the unknown tool "get_everything_now"
        When I send the tools/call request
        Then the response should indicate a tool error
