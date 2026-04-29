@notifications
Feature: Notifications
    Verify MCP server notification handling with JSON-RPC 2.0 compliance

    Scenario: Send valid notification
        Given a valid JSON-RPC notification
        When I send the notification
        Then the server should return HTTP 204 No Content
        And the server should not send a JSON-RPC response

    Scenario: Send notification for unimplemented method
        Given a notification for an unimplemented method
        When I send the notification
        Then the server should return HTTP 204 No Content
        And the server should not send an error response
