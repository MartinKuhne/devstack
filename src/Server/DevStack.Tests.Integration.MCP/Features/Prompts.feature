@prompts
Feature: Prompts
    Verify the prompts/list and prompts/get surface of the MCP server

    Scenario: The deliverable workflow prompt is advertised
        Given the MCP client is connected
        When I request the prompt list
        Then the prompts should include "deliverable_workflow"

    Scenario: The deliverable workflow prompt returns a user message
        Given the MCP client is connected
        When I resolve the prompt "deliverable_workflow"
        Then the prompt returns a user message containing "get_deliverable"

    Scenario: Requesting a prompt that the server does not provide
        Given the MCP client is connected
        When I resolve the unknown prompt "not_a_prompt"
        Then the server rejects the prompt request with an error containing "Unknown prompt"
