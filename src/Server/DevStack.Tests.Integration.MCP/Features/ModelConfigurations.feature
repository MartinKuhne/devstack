Feature: ModelConfigurations
Test GraphQL operations for ModelConfiguration entity

Scenario: Get all model configurations for a project
	Given a project with model configurations
	When I call GetModelConfigurations
	Then the response should contain all model configurations for the project

Scenario: Create a new model configuration
	Given a valid model configuration request
	When I call CreateModelConfiguration
	Then the response should contain the created model configuration
	And the API key should be encrypted

Scenario: Update an existing model configuration
	Given an existing model configuration
	When I call UpdateModelConfiguration
	Then the response should contain the updated model configuration

Scenario: Delete a model configuration
	Given an existing model configuration
	When I call DeleteModelConfiguration
	Then the response should confirm deletion
