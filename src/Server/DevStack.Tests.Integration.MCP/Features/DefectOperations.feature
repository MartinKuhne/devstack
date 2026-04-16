@devstack-tools
Feature: Defect CRUD and Status Transition Operations
    Verify MCP server devstack tools for defect management

    Scenario: Create a new defect
        Given a valid defect creation request with title "Test Defect"
        When I call devstack_createDefect
        Then the response should contain the created defect
        And the defect should have a valid ID

    Scenario: Create defect with parent feature
        Given an existing feature ID
        When I call devstack_createDefect with parentFeatureId
        Then the response should contain the defect with parent feature reference

    Scenario: Get defect by ID
        Given an existing defect ID
        When I call devstack_getDefectById with the ID
        Then the response should contain the defect details

    Scenario: Get defects
        Given existing defects in the system
        When I call devstack_getDefects
        Then the response should contain a list of defects

    Scenario: Update a defect
        Given an existing defect ID
        When I call devstack_updateDefect with updated title "Updated Defect"
        Then the response should contain the updated defect
        And the defect title should be "Updated Defect"

    Scenario: Transition defect status
        Given a defect in "planned" status
        When I call devstack_transitionDefectStatus to "in_progress"
        Then the response should contain the defect with new status
        And the status should be "in_progress"

    Scenario: Delete a defect
        Given an existing defect ID
        When I call devstack_deleteDefect with the ID
        Then the response should confirm deletion
