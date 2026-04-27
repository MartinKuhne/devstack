Feature: Deliverable Auto-Completion
  As a DevStack system
  I want deliverables to automatically transition to DONE when all agent tasks are DONE
  So that the workflow progresses without manual intervention

  Background:
    Given the API is available
    And a parent project exists

  @auto_completion
  Scenario: Deliverable transitions to DONE when all agent tasks are DONE
    Given a deliverable "All Tasks Done" type "Feature" exists
    And an agent task "Task 1" exists
    And an agent task "Task 2" exists
    When I transition the first agent task status to "DONE"
    And I transition the second agent task status to "DONE"
    Then the deliverable status should be queried and be "DONE"

  @auto_completion
  Scenario: Deliverable stays in current state when some tasks are not DONE
    Given a deliverable "Some Tasks Pending" type "Feature" exists
    And an agent task "Task 1" exists
    And an agent task "Task 2" exists
    When I transition the first agent task status to "DONE"
    And I transition the second agent task status to "IN_PROGRESS"
    Then the deliverable status should be queried and be "DRAFT"

  @auto_completion
  Scenario: Deliverable transitions to DONE when it has no tasks
    Given a deliverable with status "PLAN" type "Feature" exists
    When I call checkAndMarkDeliverableDone on the deliverable
    Then the deliverable status should be queried and be "DONE"

  @auto_completion
  Scenario: checkAndMarkDeliverableDone returns true when all tasks are DONE
    Given a deliverable "Return True" type "Feature" exists
    And an agent task "Task A" exists
    When I transition the first agent task status to "DONE"
    And I call checkAndMarkDeliverableDone on the deliverable
    Then the check result should be true

  @auto_completion
  Scenario: checkAndMarkDeliverableDone returns false when some tasks are not DONE
    Given a deliverable "Return False" type "Feature" exists
    And an agent task "Task B" exists
    When I transition the first agent task status to "IN_PROGRESS"
    And I call checkAndMarkDeliverableDone on the deliverable
    Then the check result should be false
