Feature: Deliverable Mutations
  As a DevStack API consumer
  I want to be able to create, update, transition status, and delete deliverables
  So that I can manage deliverable data

  Background:
    Given the API is available
    And a parent project exists

  @create
  Scenario: Create a new feature deliverable
    When I create a deliverable with title "Test Feature" type "Feature" and description "Feature description"
    Then the deliverable should be created successfully
    And the deliverable should exist in the database

  @create
  Scenario: Create a new defect deliverable
    When I create a deliverable with title "Test Defect" type "Defect" and description "Defect description"
    Then the deliverable should be created successfully
    And the deliverable should exist in the database

  @create
  Scenario: Create a new maintenance deliverable
    When I create a deliverable with title "Test Maintenance" type "Maintenance" and description "Maintenance description"
    Then the deliverable should be created successfully
    And the deliverable should exist in the database

  @create
  Scenario: Create a deliverable with initial status
    When I create a deliverable with title "Initial Status Feature" type "Feature" and initial status "READY"
    Then the deliverable should be created successfully
    And the deliverable status should be "READY"

  @create
  Scenario: Create a deliverable with all optional fields
    When I create a deliverable with title "Full Feature" type "Feature" description "Full description" acceptance criteria "Must pass tests" agent feedback "Good progress"
    Then the deliverable should be created successfully
    And the deliverable should exist in the database

  @update
  Scenario: Update a deliverable title
    Given a deliverable "Original Title" type "Feature" exists
    When I update the deliverable title to "Updated Title"
    Then the deliverable should be updated successfully

  @update
  Scenario: Update a deliverable description
    Given a deliverable "Original Description" type "Feature" exists
    When I update the deliverable description to "Updated description"
    Then the deliverable should be updated successfully

  @update
  Scenario: Update a deliverable acceptance criteria
    Given a deliverable "Original Criteria" type "Feature" exists
    When I update the deliverable acceptance criteria to "New acceptance criteria"
    Then the deliverable should be updated successfully

  @transition_status
  Scenario: Transition deliverable status from Planning to InProgress
    Given a deliverable with status "PLANNING" type "Feature" exists
    When I transition the deliverable status to "IN_PROGRESS"
    Then the deliverable status should be "IN_PROGRESS"

  @transition_status
  Scenario: Transition deliverable status from InProgress to Done
    Given a deliverable with status "IN_PROGRESS" type "Feature" exists
    When I transition the deliverable status to "DONE"
    Then the deliverable status should be "DONE"

  @transition_status
  Scenario: Transition deliverable status to NeedsReview
    Given a deliverable with status "IN_PROGRESS" type "Feature" exists
    When I transition the deliverable status to "NEEDS_REVIEW"
    Then the deliverable status should be "NEEDS_REVIEW"

  @transition_status
  Scenario: Transition deliverable status to Failed
    Given a deliverable with status "IN_PROGRESS" type "Feature" exists
    When I transition the deliverable status to "FAILED"
    Then the deliverable status should be "FAILED"

  @transition_status
  Scenario: Transition deliverable status to Rejected
    Given a deliverable with status "NEEDS_REVIEW" type "Feature" exists
    When I transition the deliverable status to "REJECTED"
    Then the deliverable status should be "REJECTED"

  @delete
  Scenario: Delete an existing deliverable
    Given a deliverable "To Delete" type "Feature" exists
    When I delete the deliverable
    Then the deliverable should be deleted successfully
    And the deliverable should not exist in the database

  @query
  Scenario: Query deliverable by id
    Given a deliverable "Query Test" type "Feature" exists
    When I query the deliverable by id
    Then the deliverable should be returned with correct data

  @query
  Scenario: Query deliverables by project id
    Given a deliverable "Project Query Test" type "Feature" exists
    When I query deliverables by project id
    Then the deliverables list should contain the created deliverable
