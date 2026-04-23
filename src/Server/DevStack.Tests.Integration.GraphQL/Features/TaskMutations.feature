Feature: Agent Task Mutations
  As a DevStack API consumer
  I want to be able to create, update, transition status, and delete agent tasks
  So that I can manage task data

  Background:
    Given the API is available
    And a parent project exists
    And a parent deliverable exists

  @create
  Scenario: Create a new agent task
    When I create an agent task with title "Test Task" and complexity rating 5
    Then the agent task should be created successfully
    And the agent task should exist in the database

  @create
  Scenario: Create an agent task with minimal complexity
    When I create an agent task with title "Simple Task" and complexity rating 1
    Then the agent task should be created successfully
    And the agent task should exist in the database

  @create
  Scenario: Create an agent task with maximum complexity
    When I create an agent task with title "Complex Task" and complexity rating 10
    Then the agent task should be created successfully
    And the agent task should exist in the database

  @create
  Scenario: Create an agent task with all optional fields
    When I create an agent task with title "Full Task" complexity 5 result "Success" errors null commit hash "abc123" model "gpt-4"
    Then the agent task should be created successfully
    And the agent task should exist in the database

  @create
  Scenario: Create an agent task with dependency
    Given an agent task "Dependency Task" exists
    When I create an agent task with title "Dependent Task" complexity 3 and depends on "Dependency Task"
    Then the agent task should be created successfully
    And the agent task should exist in the database

  @update
  Scenario: Update an agent task title
    Given an agent task "Original Title" exists
    When I update the agent task title to "Updated Title"
    Then the agent task should be updated successfully

  @update
  Scenario: Update an agent task complexity rating
    Given an agent task "Original Complexity" exists
    When I update the agent task complexity rating to 7
    Then the agent task should be updated successfully

  @update
  Scenario: Update an agent task result
    Given an agent task "Original Result" exists
    When I update the agent task result to "Task completed successfully"
    Then the agent task should be updated successfully

  @update
  Scenario: Update an agent task commit hash
    Given an agent task "Original Hash" exists
    When I update the agent task commit hash to "def456"
    Then the agent task should be updated successfully

  @update
  Scenario: Update an agent task model
    Given an agent task "Original Model" exists
    When I update the agent task model to "gpt-4-turbo"
    Then the agent task should be updated successfully

  @transition_status
  Scenario: Transition agent task from Ready to InProgress
    Given an agent task with status "READY" exists
    When I transition the agent task status to "IN_PROGRESS"
    Then the agent task status should be "IN_PROGRESS"

  @transition_status
  Scenario: Transition agent task from InProgress to NeedsReview
    Given an agent task with status "IN_PROGRESS" exists
    When I transition the agent task status to "NEEDS_REVIEW"
    Then the agent task status should be "NEEDS_REVIEW"

  @transition_status
  Scenario: Transition agent task from InProgress to Failed
    Given an agent task with status "IN_PROGRESS" exists
    And the agent task has errors set
    When I transition the agent task status to "FAILED"
    Then the agent task status should be "FAILED"

  @transition_status
  Scenario: Transition agent task from InProgress to Rejected
    Given an agent task with status "IN_PROGRESS" exists
    And the agent task has errors set
    When I transition the agent task status to "REJECTED"
    Then the agent task status should be "REJECTED"

  @transition_status
  Scenario: Transition agent task from NeedsReview to Done
    Given an agent task with status "NEEDS_REVIEW" exists
    And the agent task has result set
    When I transition the agent task status to "DONE"
    Then the agent task status should be "DONE"

  @transition_status
  Scenario: Transition agent task from NeedsReview to InProgress for revision
    Given an agent task with status "NEEDS_REVIEW" exists
    When I transition the agent task status to "IN_PROGRESS"
    Then the agent task status should be "IN_PROGRESS"

  @transition_status
  Scenario: Transition agent task from NeedsReview to Rejected
    Given an agent task with status "NEEDS_REVIEW" exists
    And the agent task has errors set
    When I transition the agent task status to "REJECTED"
    Then the agent task status should be "REJECTED"

  @delete
  Scenario: Delete an existing agent task
    Given an agent task "To Delete" exists
    When I delete the agent task
    Then the agent task should be deleted successfully
    And the agent task should not exist in the database

  @query
  Scenario: Query agent task by id
    Given an agent task "Query Test" exists
    When I query the agent task by id
    Then the agent task should be returned with correct data

  @query
  Scenario: Query agent tasks by deliverable id
    Given an agent task "Deliverable Query Test" exists
    When I query agent tasks by deliverable id
    Then the agent tasks list should contain the created task
