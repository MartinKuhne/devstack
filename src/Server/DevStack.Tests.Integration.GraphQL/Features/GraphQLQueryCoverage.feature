@query-coverage
Feature: GraphQL Query Coverage
  As a GraphQL client such as the AdminUi or DevStack.Agent
  I want the connection queries (projects, deliverables, agentTasks, largeLanguageModels)
  to page, filter, sort and expose relations exactly as src/schema.graphql documents
  So that the connection contract is verified instead of assumed

  Background:
    Given the query coverage GraphQL endpoint is reachable
    And a unique query coverage marker

  # ------------------------------------------------------------------ GQL-08
  @gql-08
  Scenario: GQL-08 first limits the projects page and exposes pageInfo and edges
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects with first 2 ordered by name "ASC"
    Then the "projects" connection should contain 2 nodes
    And the "projects" connection nodes should be in order "@1,@2"
    And the "projects" connection pageInfo should report hasNextPage true and hasPreviousPage false
    And the "projects" connection pageInfo cursors should be present
    And the "projects" connection edges should mirror the nodes with cursors

  @gql-08
  Scenario: GQL-08 after walks the projects connection forward to the remainder
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects with first 2 ordered by name "ASC"
    Then the "projects" connection should contain 2 nodes
    And the "projects" connection nodes should be in order "@1,@2"
    And the "projects" connection pageInfo should report hasNextPage true and hasPreviousPage false
    And the "projects" connection pageInfo cursors should be present
    When I query projects after the stored cursor with first 2 ordered by name "ASC"
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be in order "@3"
    And the "projects" connection pageInfo should report hasNextPage false and hasPreviousPage true
    And the "projects" connection nodes should not overlap the previous page
    And the "projects" connection edges should mirror the nodes with cursors

  @gql-08
  Scenario: GQL-08 last and before page the projects connection backwards
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects with last 2 ordered by name "ASC"
    Then the "projects" connection should contain 2 nodes
    And the "projects" connection nodes should be in order "@2,@3"
    And the "projects" connection pageInfo should report hasNextPage false and hasPreviousPage true
    And the "projects" connection pageInfo cursors should be present
    When I query projects before the stored cursor with last 2 ordered by name "ASC"
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be in order "@1"
    And the "projects" connection pageInfo should report hasNextPage true and hasPreviousPage false
    And the "projects" connection nodes should not overlap the previous page

  @gql-08
  Scenario Outline: GQL-08 projects are sorted by name
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects with first 10 ordered by name "<direction>"
    Then the "projects" connection should contain 3 nodes
    And the "projects" connection nodes should be in order "<expected>"
    And the "projects" connection nodes should be ordered by "name" "<direction>"
    Examples:
      | direction | expected |
      | ASC       | @1,@2,@3 |
      | DESC      | @3,@2,@1 |

  @gql-08
  Scenario Outline: GQL-08 projects are filtered by name
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects filtered by name "<operator>" "<value>"
    Then the "projects" connection should contain <count> nodes
    And the "projects" connection nodes should be "<expected>"
    Examples:
      | operator   | value    | count | expected   |
      | eq         | @1       | 1     | @1         |
      | neq        | @1       | 2     | @2,@3      |
      | contains   | BRAVO    | 1     | @2         |
      | ncontains  | BRAVO    | 2     | @1,@3      |
      | startsWith | @MARKER  | 3     | @1,@2,@3   |
      | endsWith   | @3       | 1     | @3         |
      | in         | @1,@3    | 2     | @1,@3      |
      | nin        | @1,@3    | 1     | @2         |

  @gql-08
  Scenario: GQL-08 projects are filtered with and / or combinations
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    When I query projects filtered by name contains "A" and repository contains "CHARLIE"
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be "@3"
    When I query projects filtered by name eq "@1" or name eq "@3"
    Then the "projects" connection should contain 2 nodes
    And the "projects" connection nodes should be "@1,@3"

  @gql-08
  Scenario: GQL-08 projects are filtered by nested deliverables
    Given I create query coverage projects "ALPHA,BRAVO,CHARLIE"
    And I create a query coverage deliverable "DEL1:FEATURE:DRAFT" in project 2
    And I create a query coverage deliverable "DEL2:DEFECT:DONE" in project 3
    When I query projects filtered by deliverables with status "DRAFT"
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be "@2"
    When I query projects filtered by deliverables with status "DONE"
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be "@3"
    When I query projects with no deliverables in status "DONE"
    Then the "projects" connection should contain 2 nodes
    And the "projects" connection nodes should be "@1,@2"
    When I query projects that have no deliverables
    Then the "projects" connection should contain 1 nodes
    And the "projects" connection nodes should be "@1"

  # ------------------------------------------------------------------ GQL-09
  @gql-09
  Scenario Outline: GQL-09 deliverables are filtered by status
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT,DEL2:DEFECT:DESIGN,DEL3:MAINTENANCE:IMPLEMENT"
    When I query deliverables filtered by status "<operator>" "<value>"
    Then the "deliverables" connection should contain <count> nodes
    And the "deliverables" connection nodes should be "<expected>"
    Examples:
      | operator | value        | count | expected |
      | eq       | DRAFT        | 1     | @1       |
      | in       | DRAFT,DESIGN | 2     | @1,@2    |
      | nin      | DRAFT,DESIGN | 1     | @3       |

  @gql-09
  Scenario Outline: GQL-09 deliverables are filtered by type
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT,DEL2:DEFECT:DESIGN,DEL3:SPIKE:IMPLEMENT"
    When I query deliverables filtered by type "<value>"
    Then the "deliverables" connection should contain <count> nodes
    And the "deliverables" connection nodes should be "<expected>"
    Examples:
      | value       | count | expected |
      | FEATURE     | 1     | @1       |
      | SPIKE       | 1     | @3       |

  @gql-09
  Scenario: GQL-09 deliverables are filtered by title and by project id
    Given I create query coverage projects "HOST,GUEST"
    And I create a query coverage deliverable "DEL1:FEATURE:DRAFT" in project 1
    And I create a query coverage deliverable "DEL2:DEFECT:DESIGN" in project 2
    When I query deliverables filtered by title contains "DEL1"
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be "@1"
    When I query deliverables filtered by the project id of deliverable 2
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be "@2"

  @gql-09
  Scenario: GQL-09 deliverables are filtered by nested agent tasks
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT,DEL2:DEFECT:DRAFT,DEL3:MAINTENANCE:DRAFT"
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 1
    And I create a query coverage agent task "TASK3:4:READY" in deliverable 2
    And I create a query coverage agent task "TASK1:2:DONE" in deliverable 1
    When I query deliverables filtered by agent tasks with status "DONE"
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be "@1"
    When I query deliverables with no agent tasks in status "DONE"
    Then the "deliverables" connection should contain 2 nodes
    And the "deliverables" connection nodes should be "@2,@3"
    When I query deliverables that have no agent tasks
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be "@3"

  @gql-09
  Scenario Outline: GQL-09 deliverables are sorted by title
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:IMPLEMENT,DEL2:DEFECT:DRAFT,DEL3:MAINTENANCE:DESIGN"
    When I query deliverables with first 10 ordered by title "<direction>"
    Then the "deliverables" connection should contain 3 nodes
    And the "deliverables" connection nodes should be in order "<expected>"
    And the "deliverables" connection nodes should be ordered by "title" "<direction>"
    Examples:
      | direction | expected |
      | ASC       | @1,@2,@3 |
      | DESC      | @3,@2,@1 |

  @gql-09
  Scenario: GQL-09 deliverables are sorted by status
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:IMPLEMENT,DEL2:DEFECT:DRAFT,DEL3:MAINTENANCE:DESIGN"
    When I query deliverables ordered by status "ASC"
    Then the "deliverables" connection should contain 3 nodes
    And the "deliverables" connection nodes should be in order "@2,@3,@1"
    When I query deliverables ordered by status "DESC"
    Then the "deliverables" connection should contain 3 nodes
    And the "deliverables" connection nodes should be in order "@1,@3,@2"

  @gql-09
  Scenario: GQL-09 deliverables page forward with first and after
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT,DEL2:DEFECT:DRAFT,DEL3:MAINTENANCE:DRAFT"
    When I query deliverables with first 2 ordered by title "ASC"
    Then the "deliverables" connection should contain 2 nodes
    And the "deliverables" connection nodes should be in order "@1,@2"
    And the "deliverables" connection pageInfo should report hasNextPage true and hasPreviousPage false
    And the "deliverables" connection pageInfo cursors should be present
    When I query deliverables after the stored cursor with first 2 ordered by title "ASC"
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be in order "@3"
    And the "deliverables" connection pageInfo should report hasNextPage false and hasPreviousPage true
    And the "deliverables" connection nodes should not overlap the previous page

  # ------------------------------------------------------------------ GQL-10
  @gql-10
  Scenario Outline: GQL-10 agent tasks are filtered by status
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:3:IN_PROGRESS,TASK3:4:NEEDS_REVIEW"
    When I query agent tasks filtered by status "<operator>" "<value>"
    Then the "agentTasks" connection should contain <count> nodes
    And the "agentTasks" connection nodes should be "<expected>"
    Examples:
      | operator | value             | count | expected |
      | eq       | READY             | 1     | @1       |
      | in       | READY,IN_PROGRESS | 2     | @1,@2    |
      | nin      | READY,IN_PROGRESS | 1     | @3       |

  @gql-10
  Scenario Outline: GQL-10 agent tasks are filtered by complexity rating
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:5:READY,TASK3:8:READY"
    When I query agent tasks filtered by complexity rating "<operator>" <value>
    Then the "agentTasks" connection should contain <count> nodes
    And the "agentTasks" connection nodes should be "<expected>"
    Examples:
      | operator | value | count | expected |
      | gt       | 4     | 2     | @2,@3    |
      | lt       | 4     | 1     | @1       |
      | eq       | 5     | 1     | @2       |

  @gql-10
  Scenario: GQL-10 agent tasks are filtered by title, deliverable id and project id
    Given I create query coverage projects "HOST,GUEST"
    And I create a query coverage deliverable "DEL1:FEATURE:DRAFT" in project 1
    And I create a query coverage deliverable "DEL2:DEFECT:DRAFT" in project 2
    And I create a query coverage agent task "TASK1:2:READY" in deliverable 1
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 2
    When I query agent tasks filtered by title contains "TASK1"
    Then the "agentTasks" connection should contain 1 nodes
    And the "agentTasks" connection nodes should be "@1"
    When I query agent tasks filtered by the deliverable id of task 2
    Then the "agentTasks" connection should contain 1 nodes
    And the "agentTasks" connection nodes should be "@2"
    When I query agent tasks filtered by the project id of task 1
    Then the "agentTasks" connection should contain 1 nodes
    And the "agentTasks" connection nodes should be "@1"

  @gql-10
  Scenario: GQL-10 agent tasks are filtered by dependsOnAgentTaskId
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create a query coverage agent task "TASK1:2:READY" in deliverable 1
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 1 depending on task 1
    When I query agent tasks filtered by the dependency of task 2
    Then the "agentTasks" connection should contain 1 nodes
    And the "agentTasks" connection nodes should be "@2"

  @gql-10
  Scenario: GQL-10 agent tasks are sorted by complexity rating and title
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:8:READY,TASK3:5:READY"
    When I query agent tasks ordered by complexity rating "DESC"
    Then the "agentTasks" connection should contain 3 nodes
    And the "agentTasks" connection nodes should be in order "@2,@3,@1"
    And the "agentTasks" connection nodes should be ordered by "complexityRating" "DESC"
    When I query agent tasks with first 10 ordered by title "ASC"
    Then the "agentTasks" connection nodes should be in order "@1,@2,@3"

  @gql-10
  Scenario: GQL-10 agent tasks page forward with first and after
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:3:READY,TASK3:4:READY,TASK4:5:READY"
    When I query agent tasks with first 2 ordered by title "ASC"
    Then the "agentTasks" connection should contain 2 nodes
    And the "agentTasks" connection nodes should be in order "@1,@2"
    And the "agentTasks" connection pageInfo should report hasNextPage true and hasPreviousPage false
    And the "agentTasks" connection pageInfo cursors should be present
    When I query agent tasks after the stored cursor with first 2 ordered by title "ASC"
    Then the "agentTasks" connection should contain 2 nodes
    And the "agentTasks" connection nodes should be in order "@3,@4"
    And the "agentTasks" connection pageInfo should report hasNextPage false and hasPreviousPage true
    And the "agentTasks" connection nodes should not overlap the previous page

  # ------------------------------------------------------------------ GQL-11
  @gql-11
  Scenario: GQL-11 large language models round-trip every scalar and sort by cost
    Given I create query coverage large language models "ALPHA:8:3:100,BRAVO:5:2:250"
    When I query large language models ordered by cost "DESC"
    Then the "largeLanguageModels" connection should contain 2 nodes
    And the "largeLanguageModels" connection nodes should be in order "@2,@1"
    And the "largeLanguageModels" connection nodes should be ordered by "cost" "DESC"
    And the "largeLanguageModels" connection should round-trip every created model field

  @gql-11
  Scenario Outline: GQL-11 large language models are filtered by cost
    Given I create query coverage large language models "ALPHA:8:3:100,BRAVO:5:2:250"
    When I query large language models filtered by cost "<operator>" <value>
    Then the "largeLanguageModels" connection should contain <count> nodes
    And the "largeLanguageModels" connection nodes should be "<expected>"
    Examples:
      | operator | value | count | expected |
      | eq       | 100   | 1     | @1       |
      | gt       | 150   | 1     | @2       |

  @gql-11
  Scenario: GQL-11 large language models are filtered by max complexity and model name
    Given I create query coverage large language models "ALPHA:8:3:100,BRAVO:5:2:250"
    When I query large language models filtered by max complexity gte 8
    Then the "largeLanguageModels" connection should contain 1 nodes
    And the "largeLanguageModels" connection nodes should be "@1"
    When I query large language models filtered by model contains "model-BRAVO"
    Then the "largeLanguageModels" connection should contain 1 nodes
    And the "largeLanguageModels" connection nodes should be "@2"

  # ------------------------------------------------------------------ GQL-12
  @gql-12
  Scenario: GQL-12 project by id exposes its deliverables
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT,DEL2:DEFECT:PLAN"
    When I query the query coverage project 1 with its deliverables
    Then the query coverage project relation should expose 2 deliverables "@1,@2"

  @gql-12
  Scenario: GQL-12 deliverable by id exposes its agent tasks
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:3:READY"
    When I query the query coverage deliverable 1 with its agent tasks
    Then the query coverage deliverable relation should expose 2 agent tasks "@1,@2"

  @gql-12
  Scenario: GQL-12 agent task by id exposes its foreign keys including the dependency
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create a query coverage agent task "TASK1:2:READY" in deliverable 1
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 1 depending on task 1
    When I query the query coverage agent task 2 with its relations
    Then the query coverage agent task 2 relation should reference deliverable 1 project 1 and dependency task 1

  @gql-12
  Scenario: GQL-12 relations resolve through nested object filters
    Given I create query coverage projects "HOST,GUEST"
    And I create a query coverage deliverable "DEL1:FEATURE:DRAFT" in project 1
    And I create a query coverage deliverable "DEL2:DEFECT:DRAFT" in project 2
    And I create a query coverage agent task "TASK1:2:READY" in deliverable 2
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 2 depending on task 1
    When I query deliverables filtered by the project name of deliverable 1
    Then the "deliverables" connection should contain 1 nodes
    And the "deliverables" connection nodes should be "@1"
    When I query agent tasks filtered by the deliverable title of task 1
    Then the "agentTasks" connection should contain 2 nodes
    And the "agentTasks" connection nodes should be "@1,@2"
    When I query agent tasks filtered by the project name of task 2
    Then the "agentTasks" connection should contain 2 nodes
    And the "agentTasks" connection nodes should be "@1,@2"
    When I query agent tasks filtered by the dependency title of task 2
    Then the "agentTasks" connection should contain 1 nodes
    And the "agentTasks" connection nodes should be "@2"

  # Relation objects as selected by src/AdminUi/src/graphql/queries/agentTask.graphql.
  # These three scenarios pin the observed server behaviour: the objects are null (product defect,
  # see the note in QueryCoverageSteps), only the foreign-key scalars are correct.
  @gql-12
  Scenario: GQL-12 agent task by id does not resolve its relation objects
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create a query coverage agent task "TASK1:2:READY" in deliverable 1
    And I create a query coverage agent task "TASK2:3:READY" in deliverable 1 depending on task 1
    When I query the query coverage agent task 2 with its relation objects
    Then the query coverage agent task 2 relation objects are not resolved but its foreign keys reference deliverable 1 project 1 and dependency task 1

  @gql-12
  Scenario: GQL-12 agent task connection does not resolve its relation objects
    Given I create query coverage projects "HOST"
    And I create query coverage deliverables "DEL1:FEATURE:DRAFT"
    And I create query coverage agent tasks "TASK1:2:READY,TASK2:3:READY"
    When I query agent tasks in deliverable 1 with their relation objects
    Then the query coverage agent task relation objects in deliverable 1 are not resolved but reference its project 1

  @gql-12
  Scenario: GQL-12 deliverable by id does not resolve its project object
    Given I create query coverage projects "HOST,GUEST"
    And I create a query coverage deliverable "DEL1:FEATURE:DRAFT" in project 2
    When I query the query coverage deliverable 1 with its project object
    Then the query coverage deliverable 1 project object is not resolved but its projectId references project 2
