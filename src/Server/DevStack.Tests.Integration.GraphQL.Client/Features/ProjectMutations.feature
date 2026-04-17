Feature: Project Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete projects
  So that I can manage project data

  Background:
    Given the API is available

  @create
  Scenario: Create a new project
    When I create a project with name "Test Project" and description "Test description"
    Then the project should be created successfully
    And the project should exist in the database

  @update
  Scenario: Update an existing project
    Given a project "Original Name" exists
    When I update the project name to "Updated Name"
    Then the project should be updated successfully

  @delete
  Scenario: Delete an existing project
    Given a project "To Delete" exists
    When I delete the project
    Then the project should be deleted successfully
    And the project should not exist in the database