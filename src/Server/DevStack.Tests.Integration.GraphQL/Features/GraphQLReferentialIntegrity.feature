@gql-05
Feature: GraphQL Referential Integrity
  As a DevStack API consumer
  I want to know exactly what happens when a row that is still referenced gets deleted,
  and which rows `deleteTestData` is allowed to reclaim
  So that the cascade / set-null / derived-key behaviour of the server is pinned instead of assumed

  Behaviour pinned here (observed, not assumed):
  * Project -> Deliverables and Deliverable -> AgentTasks are configured with DeleteBehavior.Cascade,
    so deleting a parent silently deletes its children and returns `true`.
  * AgentTask.DependsOnAgentTaskId is configured with DeleteBehavior.SetNull, so deleting a dependency
    leaves the dependent row behind with a cleared foreign key.
  * CreateAgentTaskHandler ignores the `projectId` of the input and derives it from the deliverable,
    so a contradicting - or non existing - projectId is accepted.
  * The dependency of a created task is only checked for existence, never for deliverable or project
    membership, so cross-deliverable and cross-project dependencies are accepted.

  Background:
    Given the API is available
    And a unique referential integrity marker

  # ------------------------------------------------------------------ GQL-05
  @gql-05
  Scenario: GQL-05 deleteProject cascades to its deliverables and their agent tasks
    Given a referential integrity project "P1"
    And a referential integrity deliverable "D1" in project "P1"
    And a referential integrity agent task "T1" in deliverable "D1"
    And a referential integrity deliverable "D2" in project "P1"
    When I delete the referential integrity project "P1"
    Then the referential integrity delete mutation should return true
    And the referential integrity project "P1" should no longer exist
    And the referential integrity deliverable "D1" should no longer exist
    And the referential integrity deliverable "D2" should no longer exist
    And the referential integrity agent task "T1" should no longer exist

  @gql-05
  Scenario: GQL-05 deleteDeliverable cascades to the agent tasks that hang off it
    Given a referential integrity project "P1"
    And a referential integrity deliverable "D1" in project "P1"
    And a referential integrity agent task "T1" in deliverable "D1"
    And a referential integrity agent task "T2" in deliverable "D1"
    When I delete the referential integrity deliverable "D1"
    Then the referential integrity delete mutation should return true
    And the referential integrity deliverable "D1" should no longer exist
    And the referential integrity agent task "T1" should no longer exist
    And the referential integrity agent task "T2" should no longer exist

  @gql-05
  Scenario: GQL-05 deleteAgentTask keeps the dependent task and clears its dependency
    Given a referential integrity project "P1"
    And a referential integrity deliverable "D1" in project "P1"
    And a referential integrity agent task "T1" in deliverable "D1"
    And a referential integrity agent task "T2" in deliverable "D1" depending on task "T1"
    When I delete the referential integrity agent task "T1"
    Then the referential integrity delete mutation should return true
    And the referential integrity agent task "T1" should no longer exist
    And the referential integrity agent task "T2" should survive with its dependency cleared

  @gql-05
  Scenario: GQL-05 a dependency on a task of another deliverable of the same project is accepted
    Given a referential integrity project "P1"
    And a referential integrity deliverable "D1" in project "P1"
    And a referential integrity deliverable "D2" in project "P1"
    And a referential integrity agent task "T1" in deliverable "D1"
    When I create a referential integrity agent task "T2" in deliverable "D2" depending on task "T1"
    Then the referential integrity mutation should report no errors
    And the referential integrity agent task "T2" should persist deliverable "D2"
    And the referential integrity agent task "T2" should persist dependency task "T1"

  @gql-05
  Scenario: GQL-05 a dependency on a task of another project is accepted and keeps the deliverable's project
    Given a referential integrity project "P1"
    And a referential integrity project "P2"
    And a referential integrity deliverable "D1" in project "P1"
    And a referential integrity deliverable "D2" in project "P2"
    And a referential integrity agent task "T1" in deliverable "D1"
    When I create a referential integrity agent task "T2" in deliverable "D2" depending on task "T1"
    Then the referential integrity mutation should report no errors
    And the referential integrity agent task "T2" should persist project "P2"
    And the referential integrity agent task "T2" should persist dependency task "T1"

  @gql-05
  Scenario: GQL-05 createAgentTask ignores a projectId that contradicts its deliverable
    Given a referential integrity project "P1"
    And a referential integrity project "P2"
    And a referential integrity deliverable "D1" in project "P1"
    When I create a referential integrity agent task "T1" in deliverable "D1" claiming project "P2"
    Then the referential integrity mutation should report no errors
    And the referential integrity agent task "T1" should persist project "P1"
    And the referential integrity agent task "T1" should persist deliverable "D1"

  @gql-05
  Scenario: GQL-05 createAgentTask accepts a projectId that does not exist at all
    Given a referential integrity project "P1"
    And a referential integrity deliverable "D1" in project "P1"
    When I create a referential integrity agent task "T1" in deliverable "D1" claiming a project that does not exist
    Then the referential integrity mutation should report no errors
    And the referential integrity agent task "T1" should persist project "P1"

  @gql-05
  Scenario: GQL-05 createDeliverable persists the projectId it was given without any cross-check
    Given a referential integrity project "P1"
    And a referential integrity project "P2"
    And a referential integrity deliverable "D1" in project "P1"
    When I create a referential integrity deliverable "D2" in project "P2"
    Then the referential integrity mutation should report no errors
    And the referential integrity deliverable "D2" should persist project "P2"
    And the referential integrity deliverable "D1" should persist project "P1"

  # ------------------------------------------------------------------ GQL-06
  @gql-06
  Scenario: GQL-06 deleteTestData reports success and removes only the marked rows
    Given a referential integrity project "MARKED"
    And a referential integrity deliverable "MARKED" in project "MARKED"
    And a referential integrity agent task "MARKED" in deliverable "MARKED"
    And a referential integrity large language model "MARKED"
    And an unmarked referential integrity project "PLAIN"
    And an unmarked referential integrity deliverable "PLAIN" in project "PLAIN"
    And an unmarked referential integrity agent task "PLAIN" in deliverable "PLAIN"
    And an unmarked referential integrity large language model "PLAIN"
    When I call deleteTestData
    Then the deleteTestData payload should report success
    And the marked referential integrity rows should all be gone
    And the unmarked referential integrity rows should all still exist
    When I delete the unmarked referential integrity rows
    Then the unmarked referential integrity rows should all be gone
