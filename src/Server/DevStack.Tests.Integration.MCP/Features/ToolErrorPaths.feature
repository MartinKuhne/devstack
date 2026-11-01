@tool-errors
Feature: Tool Error Paths
    Verify the error contracts of the DevStack MCP tools:
    hard validation errors raised as MCP protocol errors, execution errors surfaced as
    IsError results, and soft business errors returned as {"error":"..."} payloads

    Scenario Outline: Validation errors are raised as MCP protocol errors
        Given the MCP client is connected
        When I call the tool "<tool>" with arguments <arguments>
        Then the call fails with the protocol error "<expectedMessage>"

        Examples:
        | tool                  | arguments                                                              | expectedMessage |
        | get_project           | {"id":"00000000-0000-0000-0000-000000000000"}                          | Project ID is required |
        | get_project           | {"id":"<unknown>"}                                                     | Project with ID |
        | create_project        | {"name":"","repository":"https://github.com/test/repo"}                | Project name is required |
        | create_project        | {"name":"   ","repository":"https://github.com/test/repo"}             | Project name is required |
        | create_project        | {"name":"Coverage","repository":""}                                    | Repository is required |
        | create_deliverable    | {"projectId":"00000000-0000-0000-0000-000000000000","title":"t"}       | Project ID is required |
        | create_task           | {"deliverableId":"<unknown>","title":"t"}                              | Deliverable with ID |
        | get_next_deliverable  | {"projectId":"00000000-0000-0000-0000-000000000000","status":"Draft"}  | ProjectId must be provided |
        | get_next_task         | {}                                                                     | Either repositoryUrl or projectId must be provided |
        | get_next_task         | {"projectId":"00000000-0000-0000-0000-000000000000"}                   | Either repositoryUrl or projectId must be provided |

    # BUG-02 (ledger §6): get_task is in this outline because its McpProtocolException("AgentTask with ID ...
    # not found") is unreachable - GetAgentTaskByIdHandler throws InvalidOperationException first - so an unknown
    # id degrades into the generic invocation error. The sibling get_deliverable answers the same condition with
    # a clean {"error":"Deliverable not found"} payload (see the soft-error outline below), which is the
    # inconsistency this split documents rather than endorses. See the BUG-02 block in TaskTools.cs.
    Scenario Outline: Failed entity lookups inside a tool are surfaced as tool execution errors
        Given the MCP client is connected
        When I call the tool "<tool>" with arguments <arguments>
        Then the call reports a tool execution error

        Examples:
        | tool                      | arguments                                                        |
        | get_task                  | {"id":"<unknown>"}                                               |
        | update_deliverable        | {"id":"<unknown>","description":"d"}                             |
        | update_deliverable_status | {"id":"<unknown>","targetStatus":"Design","actor":"error-test"}  |
        | update_task               | {"id":"<unknown>","description":"d"}                             |
        | update_task_status        | {"id":"<unknown>","targetStatus":"InProgress","actor":"error-test"} |

    Scenario Outline: Business errors are returned as soft error payloads
        Given the MCP client is connected
        When I call the tool "<tool>" with arguments <arguments>
        Then the call returns the soft error "<expectedMessage>"

        Examples:
        | tool                 | arguments                                                              | expectedMessage |
        | get_deliverable      | {"id":"<unknown>"}                                                     | Deliverable not found |
        | get_next_deliverable | {"projectId":"<unknown>","status":"Draft"}                             | Project not found |
        | get_next_task        | {"projectId":"<unknown>"}                                              | Project not found |
        | get_next_task        | {"repositoryUrl":"https://github.com/deleteaftertest/unknown-repo"}    | Project not found |

    Scenario: get_next_deliverable reports a status without deliverables
        Given the MCP client is connected
        And an empty coverage project
        When I call get_next_deliverable for the empty coverage project with status "Deploy"
        Then the call returns the soft error "No deliverable found in Deploy status for this project"

    Scenario: get_next_task reports a project without implement deliverables
        Given the MCP client is connected
        And an empty coverage project
        When I call get_next_task for the empty coverage project
        Then the call returns the soft error "No deliverables found in Implement status for this project"

    Scenario: get_next_task ignores deliverables that are not in Implement status
        Given the MCP client is connected
        And an empty coverage project
        And a deliverable in the empty coverage project
        When I call get_next_task for the empty coverage project
        Then the call returns the soft error "No deliverables found in Implement status for this project"
