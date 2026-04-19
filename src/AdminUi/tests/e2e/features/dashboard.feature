Feature: Dashboard

    As a human software engineer or product owner
    I want to view a dashboard with deliverable counts
    So that I can monitor the software development automation process

    Background:
        Given I am on the dashboard page

    Scenario: Dashboard displays deliverable counts
        When the dashboard loads
        Then I should see the "Planning" count card
        And I should see the "Ready" count card
        And I should see the "In Progress" count card
        And I should see the "Needs Review" count card

    Scenario: Dashboard shows empty state
        Given there are no deliverables
        When the dashboard loads
        Then I should see the message "No data available yet"
