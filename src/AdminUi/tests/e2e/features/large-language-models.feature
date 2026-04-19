Feature: Large Language Models

    As an admin
    I want to create, edit, and delete large language model configurations
    So that I can configure the AI models used by the system

    Background:
        Given I am on the large language models page

    Scenario: Create a new large language model
        When I click "New Large Language Model"
        And I fill in the model name
        And I fill in the model URL
        And I fill in the API key
        And I click "Create"
        Then the large language model should be created
        And I should see a success message

    Scenario: Edit an existing large language model
        Given a large language model exists
        When I click "Edit" on the model
        And I update the model name
        And I click "Save"
        Then the large language model should be updated

    Scenario: Delete a large language model
        Given a large language model exists
        When I click "Delete" on the model
        And I confirm deletion
        Then the large language model should be deleted
