Feature: WorkflowRuns
Test GraphQL operations for WorkflowRun entity

Scenario: Get all workflow runs for a project
	Given a project with workflow runs
	When I call GetWorkflowRuns
	Then the response should contain all workflow runs for the project

Scenario: Create a new workflow run
	Given a valid workflow run request
	When I call CreateWorkflowRun
	Then the response should contain the created workflow run
	And the workflow type should be set correctly

Scenario: Update an existing workflow run
	Given an existing workflow run
	When I call UpdateWorkflowRun
	Then the response should contain the updated workflow run
	And the status should transition correctly

Scenario: Cancel a workflow run
	Given a running workflow run
	When I call CancelWorkflowRun
	Then the response should confirm cancellation
	And the status should be cancelled
