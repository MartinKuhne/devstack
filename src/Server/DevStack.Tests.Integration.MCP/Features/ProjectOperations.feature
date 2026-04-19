@devstack-tools
Feature: Project CRUD Operations
    Verify MCP server devstack tools for project management

    Scenario: Create a new project
        Given a valid project creation request with name "Test Project"
        When I call devstack_createProject
        Then the response should contain the created project
        And the project should have a valid ID

    Scenario: Get project by ID
        Given an existing project ID
        When I call devstack_getProjectById with the ID
        Then the response should contain the project details
        And the project name should match

    Scenario: Get all projects
        Given existing projects in the system
        When I call devstack_getProjects
        Then the response should contain a list of projects
        And the list should not be empty

    Scenario: Update a project
        Given an existing project ID
        When I call devstack_updateProject with updated name "Updated Project"
        Then the response should contain the updated project
        And the project name should be "Updated Project"
