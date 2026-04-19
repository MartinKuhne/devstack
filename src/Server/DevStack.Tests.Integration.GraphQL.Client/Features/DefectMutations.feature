Feature: Defect Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete defects
  So that I can manage defect data

  Background:
    Given the API is available
    And a parent project exists

  @create
  Scenario: Create a new defect
    When I create a defect with title "Test Defect" and description "Defect description"
    Then the defect should be created successfully
    And the defect should exist in the database

  @update
  Scenario: Update an existing defect
    Given a defect "Original Title" exists
    When I update the defect title to "Updated Title"
    Then the defect should be updated successfully

  @transition_status
  Scenario: Transition defect status
    Given a defect with status "Planning" exists
    When I transition the defect status to "InProgress"
    Then the defect status should be "InProgress"

  @delete
  Scenario: Delete an existing defect
    Given a defect "To Delete" exists
    When I delete the defect
    Then the defect should be deleted successfully
    And the defect should not exist in the database