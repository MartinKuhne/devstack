@error-paths
Feature: GraphQL Error Paths

  Every error and exception path reachable over the GraphQL endpoint, pinned against the
  observed server contract:

  * server exception (handler throws)  -> HTTP 200, errors[0].message "Unexpected Execution Error",
    errors[0].path [field], no extensions.code, data null (non-null field) or { field: null }
  * by-id query, unknown id            -> HTTP 200, no errors, data.field null
  * request validation                 -> HTTP 400, no data element
  * paging cap (MaxPageSize = 100)     -> HTTP 200, extensions.code HC0051, data.field null
  * cost cap (MaxTypeCost = 1000)      -> HTTP 400, extensions.code HC0047, no data element

  Background:
    Given the API is available

  # ---------------------------------------------------------------------------
  # GQL-02 - not-found / missing-entity paths
  # ---------------------------------------------------------------------------

  Scenario Outline: Mutations against a missing entity report an error, never silent null data
    When I attempt "<mutation>" against a missing entity
    Then the response should report a masked server error for "<field>" with "<data>"
    Examples:
      | mutation                     | field                       | data       |
      | updateProject                | updateProject               | null-field |
      | deleteProject                | deleteProject               | null-data  |
      | createDeliverable            | createDeliverable           | null-field |
      | updateDeliverable            | updateDeliverable           | null-data  |
      | updateDeliverableStatus      | updateDeliverableStatus     | null-data  |
      | deleteDeliverable            | deleteDeliverable           | null-data  |
      | createAgentTask              | createAgentTask             | null-data  |
      | createAgentTaskDependency    | createAgentTask             | null-data  |
      | updateAgentTask              | updateAgentTask             | null-data  |
      | updateAgentTaskStatus        | updateAgentTaskStatus       | null-data  |
      | deleteAgentTask              | deleteAgentTask             | null-data  |
      | updateLargeLanguageModel     | updateLargeLanguageModel    | null-field |
      | deleteLargeLanguageModel     | deleteLargeLanguageModel    | null-data  |
      | checkAndMarkDeliverableDone  | checkAndMarkDeliverableDone | null-data  |

  # ---------------------------------------------------------------------------
  # GQL-02b - by-id queries with an unknown id are NOT errors
  # ---------------------------------------------------------------------------

  Scenario Outline: By-id queries with an unknown id return null data and no errors
    When I query "<query>" by an unknown id
    Then the response should have HTTP status 200
    And the response should report no errors
    And the response data field "<query>" should be null
    Examples:
      | query              |
      | project            |
      | deliverable        |
      | agentTask          |
      | largeLanguageModel |

  # ---------------------------------------------------------------------------
  # GQL-03 - protocol and request-validation errors
  # ---------------------------------------------------------------------------

  Scenario Outline: Malformed documents are rejected before execution with HTTP 400 and no data
    When I attempt the protocol case "<case>"
    Then the response should report a request validation error
    And the response error message should contain "<fragment>"
    Examples:
      | case                        | fragment                                      |
      | malformed-uuid-literal      | UUID cannot parse the given literal           |
      | unknown-selection-field     | does not exist on the type                    |
      | missing-required-argument   | The argument `id` is required                 |
      | unknown-argument            | The argument `bogus` does not exist           |
      | invalid-enum-literal        | does not match the argument type              |
      | invalid-enum-variable       | cannot parse the given literal                |
      | missing-required-input      | The required input field `model` is missing   |
      | ambiguous-operation-name    | The operation name can only be omitted        |

  Scenario Outline: Built-in HotChocolate guards expose a stable extensions.code
    When I attempt the protocol case "<case>"
    Then the response error code should be "<code>"
    Examples:
      | case                   | code   |
      | syntax-error           | HC0011 |
      | paging-first-101       | HC0051 |
      | paging-last-101        | HC0051 |
      | cost-limit             | HC0047 |

  Scenario Outline: The paging cap is enforced at MaxPageSize 100 with partial data
    When I attempt the protocol case "<case>"
    Then the response should have HTTP status 200
    And the response error message should contain "maximum allowed items per page"
    And the response data field "projects" should be null
    Examples:
      | case             |
      | paging-first-101 |
      | paging-last-101  |

  Scenario: The paging cap accepts the maximum page size
    When I attempt the protocol case "paging-first-100"
    Then the response should have HTTP status 200
    And the response should report no errors
    And the response should have non-null data

  Scenario: The cost cap rejects an over-cost query with no data element
    When I attempt the protocol case "cost-limit"
    Then the response should have HTTP status 400
    And the response error message should contain "maximum allowed type cost"
    And the response should have no data element

  Scenario: Introspection is enabled and succeeds
    When I attempt the protocol case "introspection"
    Then the response should have HTTP status 200
    And the response should report no errors
    And the response should have non-null data

  # ---------------------------------------------------------------------------
  # GQL-04 - domain validation
  # ---------------------------------------------------------------------------

  Scenario Outline: Domain validation rejects invalid input with a masked server error
    Given a project, deliverable and agent task exist for error testing
    When I attempt the invalid input case "<case>" on the existing entities
    Then the response should report a masked server error for "<field>" with "<data>"
    Examples:
      | case                            | field             | data       |
      | create-project-blank-name       | createProject     | null-field |
      | create-project-201-name         | createProject     | null-field |
      | create-deliverable-empty-title  | createDeliverable | null-field |
      | create-deliverable-blank-title  | createDeliverable | null-field |
      | create-deliverable-201-title    | createDeliverable | null-field |
      | update-deliverable-empty-title  | updateDeliverable | null-data  |
      | update-deliverable-201-title    | updateDeliverable | null-data  |
      | create-agent-task-empty-title   | createAgentTask   | null-data  |
      | create-agent-task-201-title     | createAgentTask   | null-data  |
      | create-agent-task-complexity-0  | createAgentTask   | null-data  |
      | create-agent-task-complexity-11 | createAgentTask   | null-data  |
      | update-agent-task-empty-title   | updateAgentTask   | null-data  |
      | update-agent-task-301-title     | updateAgentTask   | null-data  |
      | update-agent-task-complexity-0  | updateAgentTask   | null-data  |
      | update-agent-task-complexity-11 | updateAgentTask   | null-data  |

  Scenario: A 201 character task title is accepted on update although create rejects it
    # Pins the asymmetry between CreateAgentTaskHandler (max 200) and AgentTask.UpdateMetadata (max 300).
    Given a project, deliverable and agent task exist for error testing
    When I attempt the invalid input case "update-agent-task-201-title" on the existing entities
    Then the response should have HTTP status 200
    And the response should report no errors
    And the response data field "updateAgentTask" should not be null

  Scenario Outline: Deliverable type parsing is case insensitive
    Given a project, deliverable and agent task exist for error testing
    When I attempt the invalid input case "<case>" on the existing entities
    Then the response should have HTTP status 200
    And the response should report no errors
    And the response data field "createDeliverable" should not be null
    Examples:
      | case                          |
      | create-deliverable-type-lc    |
      | create-deliverable-type-mix   |
      | create-deliverable-type-spike |

  Scenario: An unknown deliverable type string is rejected as a masked server error
    Given a project, deliverable and agent task exist for error testing
    When I attempt the invalid input case "create-deliverable-type-bogus" on the existing entities
    Then the response should report a masked server error for "createDeliverable" with "null-field"

  Scenario: A numeric deliverable type string slips past Enum.Parse and fails serialization
    # Enum.Parse accepts numeric strings, so the failure surfaces as a leaf-value error, not a masked one.
    Given a project, deliverable and agent task exist for error testing
    When I attempt the invalid input case "create-deliverable-type-numeric" on the existing entities
    Then the response should have HTTP status 200
    And the response error message should contain "cannot serialize the given value"
    And the response error code should be "EXEC_INVALID_LEAF_VALUE"
    And the response data field "createDeliverable" should be null

  # ---------------------------------------------------------------------------
  # GQL-07 - error contract hygiene
  # ---------------------------------------------------------------------------

  Scenario Outline: Server exceptions are masked and leak no internals
    When I attempt "<mutation>" against a missing entity
    Then the response should report a masked server error for "<field>" with "<data>"
    And the response should not leak exception details
    Examples:
      | mutation          | field             | data       |
      | updateProject     | updateProject     | null-field |
      | deleteProject     | deleteProject     | null-data  |
      | updateAgentTask   | updateAgentTask   | null-data  |
      | createDeliverable | createDeliverable | null-field |

  Scenario: Not-found mutations share one consistent error classification
    When I attempt three not-found mutations
    Then the three responses should share the same error classification

  Scenario Outline: Masked execution errors carry no extensions code while validation guards do
    When I attempt "<mutation>" against a missing entity
    Then the response error code should be absent
    Examples:
      | mutation        |
      | deleteProject   |
      | deleteAgentTask |
