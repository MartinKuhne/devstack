Feature: Large Language Model Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete large language models
  So that I can manage AI model configurations

  Background:
    Given the API is available

  @create
  Scenario: Create a new large language model
    When I create a large language model with url "https://api.example.com" and model "gpt-4" and api key "test-key-123"
    Then the large language model should be created successfully
    And the large language model should exist in the database

  @update
  Scenario: Update an existing large language model
    Given a large language model exists
    When I update the large language model model alias to "Updated Alias"
    Then the large language model should be updated successfully

  @delete
  Scenario: Delete an existing large language model
    Given a large language model exists
    When I delete the large language model
    Then the large language model should be deleted successfully
    And the large language model should not exist in the database
