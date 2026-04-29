@initialize
Feature: Initialize
    Verify MCP server connection and capability negotiation

    Scenario: Connect and initialize server
        Given the MCP server is available
        When I initialize the client
        Then the server should return its protocol version
        And the server should return its implementation info
        And the server should advertise tools capability
