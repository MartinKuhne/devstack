Feature: Project Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete projects
  So that I can manage project data

  Background:
    Given the API is available

  @create
  Scenario: Create a new project with name and description
    When I create a project with name "Test Project" and description "Test description"
    Then the project should be created successfully
    And the project should exist in the database

  @create
  Scenario: Create a new project with minimal data
    When I create a project with name "Minimal Project" and no description
    Then the project should be created successfully
    And the project should exist in the database

  @update
  Scenario: Update an existing project name
    Given a project "Original Name" exists
    When I update the project name to "Updated Name"
    Then the project should be updated successfully

  @update
  Scenario: Update an existing project description
    Given a project "Original Description" exists
    When I update the project description to "New description"
    Then the project should be updated successfully

  @update
  Scenario: Update an existing project repository
    Given a project "Original Repo" exists
    When I update the project repository to "https://github.com/test/repo"
    Then the project should be updated successfully

  @delete
  Scenario: Delete an existing project
    Given a project "To Delete" exists
    When I delete the project
    Then the project should be deleted successfully
    And the project should not exist in the database

  @query
  Scenario: Query project by id
    Given a project "Query Test" exists
    When I query the project by id
    Then the project should be returned with correct data
