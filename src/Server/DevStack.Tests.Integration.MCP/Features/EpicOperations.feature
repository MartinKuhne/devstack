@devstack-tools
Feature: Epic CRUD Operations
    Verify MCP server devstack tools for epic management

    Scenario: Create a new epic
        Given a valid epic creation request with title "Test Epic"
        When I call devstack_createEpic
        Then the response should contain the created epic
        And the epic should have a valid ID

    Scenario: Get epic by ID
        Given an existing epic ID
        When I call devstack_getEpicById with the ID
        Then the response should contain the epic details

    Scenario: Get epics with title filter
        Given existing epics in the system
        When I call devstack_getEpics with title filter "Test"
        Then the response should contain filtered epics
        And all epics should contain "Test" in the title

    Scenario: Update an epic
        Given an existing epic ID
        When I call devstack_updateEpic with updated title "Updated Epic"
        Then the response should contain the updated epic
        And the epic title should be "Updated Epic"

    Scenario: Delete an epic
        Given an existing epic ID
        When I call devstack_deleteEpic with the ID
        Then the response should confirm deletion
