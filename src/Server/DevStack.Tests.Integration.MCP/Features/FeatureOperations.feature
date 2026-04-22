@devstack-tools
Feature: Feature CRUD and Status Transition Operations
    Verify MCP server devstack tools for deliverable management

    Scenario: Create a new deliverable
        Given a valid deliverable creation request with title "Test Deliverable"
        When I call create_deliverable
        Then the response should contain the created deliverable
        And the deliverable should have a valid ID
        And the deliverable status should be "Ready"

    Scenario: Update a deliverable
        Given an existing deliverable ID
        When I call update_deliverable with updated description "Updated Description"
        Then the response should contain the updated deliverable

    Scenario: Transition deliverable status
        Given an existing deliverable ID
        And a deliverable in "ready" status
        When I call update_deliverable_state to "in_progress"
        Then the response should contain the deliverable with new status
        And the status should be "InProgress"
