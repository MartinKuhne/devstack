Feature: Projects

    As an admin
    I want to create, edit, and delete projects
    So that I can manage the software development automation targets

    Background:
        Given I am on the projects page

    Scenario: Create a new project
        When I click "New Project"
        And I fill in the project name
        And I fill in the project description
        And I click "Create"
        Then the project should be created
        And I should see a success message

    Scenario: Edit an existing project
        Given a project exists
        When I click "Edit" on the project
        And I update the project name
        And I click "Save"
        Then the project should be updated

    Scenario: Delete a project
        Given a project exists
        When I click "Delete" on the project
        And I confirm deletion
        Then the project should be deleted
