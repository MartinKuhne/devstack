Feature: Feature Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete features
  So that I can manage feature data

  Background:
    Given the API is available
    And a parent project exists

  @create
  Scenario: Create a new feature
    When I create a feature with title "Test Feature" and description "Feature description"
    Then the feature should be created successfully
    And the feature should exist in the database

  @update
  Scenario: Update an existing feature
    Given a feature "Original Title" exists
    When I update the feature title to "Updated Title"
    Then the feature should be updated successfully

  @transition_status
  Scenario: Transition feature status
    Given a feature with status "Planning" exists
    When I transition the feature status to "InProgress"
    Then the feature status should be "InProgress"

  @delete
  Scenario: Delete an existing feature
    Given a feature "To Delete" exists
    When I delete the feature
    Then the feature should be deleted successfully
    And the feature should not exist in the database