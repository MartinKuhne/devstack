# ADR-006: Event-Driven Architecture

## Status
Proposed

## Context
The current agent process implementation uses polling to check for workflow status changes and task updates. This approach has scalability limitations and introduces unnecessary latency. We need to transition to an event-driven architecture where workers subscribe to webhook events instead of continuously polling.

## Decision
Implement a webhook-based event-driven architecture with the following components:

### Webhook Endpoint
- Add a webhook endpoint in the API for status change events
- Events include workflow status changes, task completion/failure, and other significant state transitions
- Secure the webhook endpoint with signature validation

### Worker Subscription
- Modify the worker to subscribe to webhook events instead of polling
- Implement event handlers for different event types
- Maintain backward compatibility during transition period

### Event Types
- WorkflowStarted: When a workflow begins execution
- WorkflowCompleted: When a workflow finishes successfully
- WorkflowFailed: When a workflow encounters an error
- TaskStarted: When a task begins execution
- TaskCompleted: When a task finishes successfully
- TaskFailed: When a task encounters an error
- SecretUpdated: When encrypted secrets are modified

## Consequences

### Positive
- Reduced latency in workflow execution
- Improved scalability by eliminating continuous polling
- Better resource utilization (workers idle when no events)
- Real-time status updates in the UI

### Negative
- Increased complexity in event handling
- Need for webhook security measures
- Potential for event loss if webhook endpoint is unavailable
- Requires reliable event delivery mechanism

## Implementation Notes
1. Use HTTP POST for webhook delivery with JSON payload
2. Implement idempotency in event processing to handle duplicate events
3. Add retry mechanism with exponential backoff for failed webhook deliveries
4. Consider using a message queue (like RabbitMQ or Apache Kafka) for guaranteed delivery in future iterations
5. Maintain polling as fallback mechanism during transition period

## Related Decisions
- This ADR supersedes the polling approach mentioned in specs/plan-agent-process.md Step 6.1
- Future work may include migrating to a dedicated message queue system