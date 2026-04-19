Feature: Agent Tasks

    As an admin
    I want to create, edit, delete, and change the status of agent tasks
    So that I can manage the AI agent execution workflow

    Background:
        Given I am on the agent tasks page

    Scenario: Create a new agent task
        When I click "New Agent Task"
        And I fill in the agent task title
        And I select a deliverable
        And I click "Create"
        Then the agent task should be created
        And I should see a success message

    Scenario: Edit an existing agent task
        Given an agent task exists
        When I navigate to the agent task detail page
        And I click "Edit"
        And I update the agent task title
        And I click "Save"
        Then the agent task should be updated

    Scenario: Change agent task status
        Given an agent task exists with status "READY"
        When I navigate to the agent task detail page
        And I change the status to "IN_PROGRESS"
        Then the agent task status should be "IN_PROGRESS"

    Scenario: Delete an agent task
        Given an agent task exists
        When I navigate to the agent task detail page
        And I delete the agent task
        Then the agent task should be deleted
