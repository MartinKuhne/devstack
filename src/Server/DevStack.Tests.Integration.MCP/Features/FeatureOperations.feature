@devstack-tools
Feature: Feature CRUD and Status Transition Operations
    Verify MCP server devstack tools for feature management

    Scenario: Create a new feature
        Given a valid feature creation request with title "Test Feature"
        When I call devstack_createFeature
        Then the response should contain the created feature
        And the feature should have a valid ID

    Scenario: Get feature by ID
        Given an existing feature ID
        When I call devstack_getFeatureById with the ID
        Then the response should contain the feature details

    Scenario: Get features with filters
        Given existing features in the system
        When I call devstack_getFeatures with projectId filter
        Then the response should contain filtered features
        And all features should belong to the specified project

    Scenario: Update a feature
        Given an existing feature ID
        When I call devstack_updateFeature with updated title "Updated Feature"
        Then the response should contain the updated feature
        And the feature title should be "Updated Feature"

    Scenario: Transition feature status
        Given a feature in "planned" status
        When I call devstack_transitionFeatureStatus to "in_progress"
        Then the response should contain the feature with new status
        And the status should be "in_progress"

    Scenario: Get valid status transitions
        Given a feature in "planned" status
        When I call devstack_getValidStatusTransitions
        Then the response should contain valid transitions
        And "in_progress" should be a valid transition

    Scenario: Delete a feature
        Given an existing feature ID
        When I call devstack_deleteFeature with the ID
        Then the response should confirm deletion
