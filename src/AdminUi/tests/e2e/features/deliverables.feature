Feature: Deliverables

    As an admin
    I want to create, edit, delete, and change the status of deliverables
    So that I can manage the work items in the development process

    Background:
        Given I am on the deliverables page

    Scenario: Create a new deliverable
        When I click "New Deliverable"
        And I fill in the deliverable title
        And I fill in the description
        And I click "Create"
        Then the deliverable should be created
        And I should see a success message

    Scenario: Edit an existing deliverable
        Given a deliverable exists
        When I navigate to the deliverable detail page
        And I click "Edit"
        And I update the deliverable title
        And I click "Save"
        Then the deliverable should be updated

    Scenario: Change deliverable status
        Given a deliverable exists with status "PLANNING"
        When I navigate to the deliverable detail page
        And I change the status to "READY"
        Then the deliverable status should be "READY"

    Scenario: Delete a deliverable
        Given a deliverable exists
        When I navigate to the deliverable detail page
        And I delete the deliverable
        Then the deliverable should be deleted
