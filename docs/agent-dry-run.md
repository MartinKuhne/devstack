# Agent Process Dry-Run Mode

Dry-run mode allows you to test the agent process scheduler, queue, and workflow execution without consuming LLM API credits or making actual API calls to external services.

## Overview

When dry-run mode is enabled:
- The scheduler continues to poll and queue workflow jobs normally
- Workers process jobs from the queue
- LLM calls are short-circuited with synthetic success responses
- Workflow state changes are still persisted to the database
- The UI can observe workflow runs as if they were real executions
- No actual code changes or external API calls are made

## Configuration

Set the `DRY_RUN` environment variable to `true`:

```bash
DRY_RUN=true
```

### Example with docker-compose

```yaml
services:
  agent-worker:
    build: .
    environment:
      - DRY_RUN=true
      - GRAPHQL_API_URL=http://localhost:4000/graphql
      - REDIS_URL=redis://localhost:6379
```

### Example with direct execution

```bash
DRY_RUN=true npm run start
```

## What Happens in Dry-Run Mode

### Planner Workflow
Returns synthetic task breakdown:
```json
{
  "plan": "Dry-run: Simulated planning for feature",
  "tasks": [
    {
      "title": "Dry-run task 1",
      "deliverable": "Simulated deliverable",
      "acceptanceCriteria": "Dry-run acceptance criteria",
      "risks": "None",
      "complexityRating": 3,
      "requiredFollowUps": ""
    }
  ],
  "openQuestions": [],
  "securityImpact": "None",
  "performanceImpact": "None",
  "testPlan": "Standard testing",
  "deploymentPlan": "Standard deployment"
}
```

### DevLead Workflow
Returns synthetic branch and action plan without creating actual branches.

### Coder Workflow
Returns synthetic file modifications without changing any files.

### Tester Workflow
Returns synthetic build and test success.

### Architect Workflow
Returns synthetic recommendations without creating actual features.

## Use Cases

1. **Testing Queue Behavior**: Verify that jobs are queued, processed, and completed correctly
2. **UI Testing**: Test the admin UI workflow visualization without LLM costs
3. **Integration Testing**: End-to-end testing of the entire agent pipeline
4. **Development**: Faster iteration during development without waiting for LLM responses
5. **Demonstration**: Show the system behavior to stakeholders without incurring costs

## Limitations

- Synthetic responses are generic and may not reflect real LLM output quality
- No actual code generation or modification occurs
- No real branch creation or PR generation
- Token usage and cost tracking will show zero or minimal values

## Disabling Dry-Run Mode

To return to normal operation, set `DRY_RUN=false` or remove the variable:

```bash
DRY_RUN=false npm run start
```

Or simply don't set the variable (defaults to false).

## Troubleshooting

### Jobs not completing in dry-run mode
Ensure all required environment variables are set correctly. Dry-run mode still requires valid GraphQL and Redis configuration.

### Synthetic responses not appearing
Check that the `DRY_RUN` environment variable is properly passed to the process. Use `npm run start` or check your docker-compose configuration.

## Future Enhancements

Potential improvements for dry-run mode:
- Configurable synthetic response templates
- Mock data fixtures for specific test scenarios
- Recording and replay of real workflow runs
- Dry-run mode for specific workflows only
