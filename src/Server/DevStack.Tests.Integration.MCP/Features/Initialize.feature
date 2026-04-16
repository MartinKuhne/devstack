@initialize
Feature: Initialize Method
    Verify MCP server initialize method returns correct protocol version and capabilities

    Scenario: Initialize with valid request
        Given a valid initialize request with protocol version "2024-11-05"
        When I send the initialize request
        Then the response should contain protocol version "2024-11-05"
        And the response should contain server name "DevStack MCP Server"
        And the response should contain tools capability
