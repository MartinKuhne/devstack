@notifications
Feature: Notifications
    Verify MCP server notification handling with Streamable HTTP transport

    Scenario: Send initialized notification
        Given the MCP client is connected
        When I send the notifications/initialized notification
        Then the server should accept the notification
