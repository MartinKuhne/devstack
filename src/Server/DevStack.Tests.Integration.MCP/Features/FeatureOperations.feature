@devstack-tools
Feature: Feature CRUD and Status Transition Operations
    Verify MCP server devstack tools for deliverable management with JSON-RPC 2.0 compliance

    Scenario: Create a new deliverable
        Given a valid deliverable creation request with title "Test Deliverable"
        When I call create_deliverable
        Then the response should contain the created deliverable
        And the deliverable should have a valid ID
        And the deliverable status should be "Ready"
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field

    Scenario: Update a deliverable
        Given an existing deliverable ID
        When I call update_deliverable with updated description "Updated Description"
        Then the response should contain the updated deliverable
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field

    Scenario: Transition deliverable status
        Given an existing deliverable ID
        And a deliverable in "ready" status
        When I call update_deliverable_state to "in_progress"
        Then the response should contain the deliverable with new status
        And the status should be "InProgress"
        And the response should have jsonrpc field "2.0"
        And the response should echo the request id
        And the response should have a result field
        And the response should not have an error field
