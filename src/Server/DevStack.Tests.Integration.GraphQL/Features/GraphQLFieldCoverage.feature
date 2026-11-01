@gql-13
Feature: GraphQL Field Coverage
  As a DevStack API consumer
  I want every writable input field to survive a write and a re-query unchanged
  So that the document fields, the agent task metrics and the model configuration are known to round-trip

  Values asserted here were never sent by any earlier scenario: multi-line text, markdown-ish
  punctuation, tab characters, non-ASCII text, the token counters of an agent task and the
  <c>apiKey</c> / <c>cost</c> pair of a large language model.

  Background:
    Given the API is available
    And a field coverage marker
    And a field coverage project

  # ------------------------------------------------------------------ GQL-13
  @gql-13
  Scenario: GQL-13 createDeliverable persists the design document
    When I create a field coverage deliverable with design "# Design\nline1\nline2\tindented"
    Then the field coverage mutation should report no errors
    When I query the field coverage deliverable
    Then the field coverage deliverable field "design" should equal the value that was sent

  @gql-13
  Scenario Outline: GQL-13 updateDeliverable round-trips <field>
    Given a field coverage deliverable
    When I update the field coverage deliverable field "<field>" to "<value>"
    Then the field coverage mutation should report no errors
    When I query the field coverage deliverable
    Then the field coverage deliverable field "<field>" should equal the value that was sent
    Examples:
      | field              | value                                     |
      | title              | @MARKER title #heading line1\nline2       |
      | description        | line1\nline2 with **bold** and `code`     |
      | acceptanceCriteria | # Acceptance\n- [ ] given\n- [ ] when     |
      | securityImpact     | # Security\nno PII, no secrets            |
      | performanceImpact  | p95 < 200ms\npeak 1_200 rps               |
      | testPlan           | ```\nAssert.True(x);\n```                 |
      | deploymentPlan     | canary 10%\nrollback: revert #42          |
      | blocking           | blocked by #7 - waiting for @team         |
      | design             | # Design\n## Overview\n- one\n- two       |

    # agentFeedback and executionPlan are not part of the examples above: updateDeliverable does not
    # round-trip them, it writes each of them into the other column (scenario below).

  @gql-13
  # KNOWN DEFECT BUG-07 (doc/planning/integration-test-coverage-ledger.md §6), pinned rather than fixed by this
  # suite. Mutation.UpdateDeliverableAsync builds
  # UpdateDeliverableCommand with positional arguments; UpdateDeliverableInput declares AgentFeedback before
  # ExecutionPlan, UpdateDeliverableCommand declares ExecutionPlan before AgentFeedback, so the two values are
  # swapped on the way to the entity. Both assertions fail once the API maps the fields correctly.
  Scenario: GQL-13 DEFECT updateDeliverable crosses agentFeedback and executionPlan
    Given a field coverage deliverable
    When I update the field coverage deliverable with agentFeedback "the agent feedback" and executionPlan "the execution plan"
    Then the field coverage mutation should report no errors
    When I query the field coverage deliverable
    Then the field coverage deliverable field "executionPlan" should hold the value that was sent for "agentFeedback"
    And the field coverage deliverable field "agentFeedback" should hold the value that was sent for "executionPlan"

  @gql-13
  # Every document field is written in one call. The nine fields that map correctly are compared with the value
  # sent for them; agentFeedback and executionPlan are compared with the value of the column they are observed
  # to be written to (see the defect scenario above).
  Scenario: GQL-13 updateDeliverable round-trips every document field in a single call
    Given a field coverage deliverable
    When I update the field coverage deliverable with every document field
    Then the field coverage mutation should report no errors
    When I query the field coverage deliverable
    Then the field coverage deliverable should expose every document field that was sent

  @gql-13
  Scenario Outline: GQL-13 updateAgentTask round-trips <field>
    Given a field coverage deliverable
    And a field coverage agent task
    When I update the field coverage agent task field "<field>" to "<value>"
    Then the field coverage mutation should report no errors
    When I query the field coverage agent task
    Then the field coverage agent task field "<field>" should equal the value that was sent
    Examples:
      | field                      | value                      |
      | promptTokens               | 12345                      |
      | completionTokens           | 6789                       |
      | executionDurationInSeconds | 3600                       |
      | errors                     | line1\nline2: frame #3     |
      | commitHash                 | 0123abc4567def89           |
      | agent                      | agent-#7 / model-x         |

  @gql-13
  # SEC-01 (ledger §6): this scenario pins the credential ECHO, not a contract we want. apiKey is published in
  # src/schema.graphql and returned verbatim by both largeLanguageModel queries, so the test would have to be
  # inverted (assert a masked value or an absent field) as part of fixing SEC-01.
  Scenario: GQL-13 updateLargeLanguageModel round-trips apiKey, cost and modelAlias
    Given a field coverage large language model
    When I update the field coverage large language model with apiKey "sk-test_abc123-XYZ #7 line1\nline2" cost 4242 and modelAlias "@MARKER alias-renamed"
    Then the field coverage mutation should report no errors
    When I query the field coverage large language model
    Then the field coverage large language model should return the sent apiKey, cost and modelAlias verbatim

  # ------------------------------------------------------------------ GQL-14
  @gql-14
  Scenario Outline: GQL-14 a deliverable of <type> persists and is found by its type filter
    When I create a field coverage deliverable of type "<type>"
    Then the field coverage mutation should report no errors
    And the field coverage deliverable should have type "<persisted>"
    When I query field coverage deliverables filtered by type "<persisted>"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "<type>"
    Examples:
      | type        | persisted   |
      | FEATURE     | FEATURE     |
      | DEFECT      | DEFECT      |
      | MAINTENANCE | MAINTENANCE |
      | SPIKE       | SPIKE       |

  @gql-14
  Scenario Outline: GQL-14 the type string <type> is parsed case-insensitively
    When I create a field coverage deliverable of type "<type>"
    Then the field coverage mutation should report no errors
    And the field coverage deliverable should have type "<persisted>"
    When I query field coverage deliverables filtered by type "<persisted>"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "<type>"
    Examples:
      | type        | persisted   |
      | feature     | FEATURE     |
      | Spike       | SPIKE       |
      | dEfEcT      | DEFECT      |
      | maintenance | MAINTENANCE |

  @gql-14
  Scenario: GQL-14 deliverables of every type coexist and each is found by its own type filter
    When I create a field coverage deliverable of type "FEATURE"
    And I create a field coverage deliverable of type "DEFECT"
    And I create a field coverage deliverable of type "MAINTENANCE"
    And I create a field coverage deliverable of type "SPIKE"
    Then the field coverage mutation should report no errors
    When I query field coverage deliverables filtered by type "FEATURE"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "FEATURE"
    When I query field coverage deliverables filtered by type "DEFECT"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "DEFECT"
    When I query field coverage deliverables filtered by type "MAINTENANCE"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "MAINTENANCE"
    When I query field coverage deliverables filtered by type "SPIKE"
    Then the field coverage deliverable connection should contain 1 nodes
    And the field coverage deliverable connection should contain only the deliverable of type "SPIKE"
