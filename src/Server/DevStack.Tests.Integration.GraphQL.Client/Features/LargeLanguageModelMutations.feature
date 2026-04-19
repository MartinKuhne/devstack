Feature: Large Language Model Mutations
  As a DevStack API consumer
  I want to be able to create, update, and delete large language models
  So that I can manage AI model configurations

  Background:
    Given the API is available

  @create
  Scenario: Create a new large language model with required fields
    When I create a large language model with url "https://api.example.com" model "gpt-4" api key "test-key-123" and max complexity 10
    Then the large language model should be created successfully
    And the large language model should exist in the database

  @create
  Scenario: Create a large language model with model alias
    When I create a large language model with url "https://api.openai.com/v1" model "gpt-4-turbo" api key "key-456" max complexity 8 and alias "Turbo Model"
    Then the large language model should be created successfully
    And the large language model should exist in the database

  @create
  Scenario: Create a large language model with max concurrency
    When I create a large language model with url "https://api.anthropic.com" model "claude-3" api key "sk-ant-123" max complexity 10 and max concurrency 5
    Then the large language model should be created successfully
    And the large language model should exist in the database

  @update
  Scenario: Update a large language model model alias
    Given a large language model exists
    When I update the large language model model alias to "Updated Alias"
    Then the large language model should be updated successfully

  @update
  Scenario: Update a large language model url
    Given a large language model exists
    When I update the large language model url to "https://new-api.example.com"
    Then the large language model should be updated successfully

  @update
  Scenario: Update a large language model model name
    Given a large language model exists
    When I update the large language model model name to "gpt-4-turbo"
    Then the large language model should be updated successfully

  @update
  Scenario: Update a large language model max complexity
    Given a large language model exists
    When I update the large language model max complexity to 8
    Then the large language model should be updated successfully

  @update
  Scenario: Update a large language model max concurrency
    Given a large language model exists
    When I update the large language model max concurrency to 10
    Then the large language model should be updated successfully

  @delete
  Scenario: Delete an existing large language model
    Given a large language model exists
    When I delete the large language model
    Then the large language model should be deleted successfully
    And the large language model should not exist in the database

  @query
  Scenario: Query large language model by id
    Given a large language model exists
    When I query the large language model by id
    Then the large language model should be returned with correct data

  @query
  Scenario: Query all large language models
    Given multiple large language models exist
    When I query all large language models
    Then the large language models list should contain the created models
