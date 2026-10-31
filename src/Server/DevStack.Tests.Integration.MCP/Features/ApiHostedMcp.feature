@api-hosted-mcp
Feature: MCP Endpoint Hosted By DevStack.Api
    DevStack.Api maps the same MCP tools, prompts and resources on /mcp as the standalone
    DevStack.Mcp host, but registers a shorter request-filter pipeline: it adds
    McpToolLoggingFilter only and omits McpArgumentNormalizationFilter.
    The API image runs EF migrations at start-up, so the scenarios create their own
    [DeleteAfterTest] project instead of relying on the assembly seed.

    Scenario: tools/list matches the tool set of the standalone MCP host
        Given the API-hosted MCP client is connected
        When I request the API-hosted tool list
        Then the API-hosted tools are exactly the tools of the standalone MCP host

    Scenario: create_project and get_project round trip over the API host
        Given the API-hosted MCP client is connected
        When I create an API-hosted project named "Roundtrip"
        Then the API-hosted project can be read back with get_project

    Scenario: a hard validation error surfaces the same way on both hosts
        Given the MCP client is connected
        And the API-hosted MCP client is connected
        When I call get_project with the zero guid on both hosts
        Then both hosts report the hard error as a protocol error containing "Project ID is required"

    Scenario Outline: the API image answers its health probes
        Given the API-hosted MCP client is connected
        When I probe the API-hosted endpoint "<path>"
        Then the API-hosted endpoint responds with 200

        Examples:
        | path    |
        | /health |
        | /ready  |

    Scenario: an array argument is normalised by the standalone host only
        Given the MCP client is connected
        And the API-hosted MCP client is connected
        When I create a project with an array name on both hosts
        Then the standalone MCP host normalises the array name into a string
        But the API-hosted MCP host rejects the array name
