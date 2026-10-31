@resources
Feature: Resources
    Verify the resources/list and resources/read surface of the MCP server

    Scenario: The server info resource is advertised
        Given the MCP client is connected
        When I request the resource list
        Then the resources should include the server info resource

    Scenario: The server info resource returns its content
        Given the MCP client is connected
        When I read the server info resource
        Then the server info resource content contains "DevStack MCP Server"

    Scenario: Reading a resource that the server does not provide
        Given the MCP client is connected
        When I read the unknown resource "resource://mcp/not_a_resource"
        Then the server rejects the resource request with an error containing "Unknown resource URI"
