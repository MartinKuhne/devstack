Feature: Task Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete tasks
  So that I can manage task data

  Background:
    Given the API is available
    And a parent project exists
    And a parent feature exists

  @create
  Scenario: Create a new task
    When I create a task with title "Test Task" and complexity rating 5
    Then the task should be created successfully
    And the task should exist in the database

  @update
  Scenario: Update an existing task
    Given a task "Original Title" exists
    When I update the task title to "Updated Title" and complexity rating to 7
    Then the task should be updated successfully

  @transition_status
  Scenario: Transition task status
    Given a task with status "Todo" exists
    When I transition the task status to "Done"
    Then the task status should be "Done"

  @delete
  Scenario: Delete an existing task
    Given a task "To Delete" exists
    When I delete the task
    Then the task should be deleted successfully
    And the task should not exist in the database