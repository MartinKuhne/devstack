# ADR-008: Long-Horizon Context Management for Large Features

## Status
Accepted

## Context
As the Agent Process handles increasingly large features composed of many sequential tasks, there's a risk of exceeding model context limits. Early task results may contain important context that later tasks need, but simply including all prior task outputs in the context window becomes infeasible as the feature grows.

## Decision
Implement a task context summarization strategy where completed task results are condensed into concise summaries that preserve essential information for subsequent tasks while minimizing token usage.

### Implementation Approach
1. **Task Result Summarization**: After each task completion, generate a summary of key outcomes, decisions, and artifacts produced
2. **Context Window Management**: Maintain a rolling window of recent task summaries plus the full context of the current task
3. **Hierarchical Summarization**: For very long feature chains, implement multi-level summarization (daily/weekly digests)
4. **Selective Detail Preservation**: Preserve full details for critical artifacts (code, configuration) while summarizing procedural steps

### Storage and Retrieval
- **Storage Location**: Store summaries in the feature's metadata field in the database
- **Retrieval Mechanism**: Workflow planner automatically injects relevant context summaries when planning subsequent tasks
- **Format**: JSON structure with sections for decisions, artifacts, open questions, and blockers
- **Versioning**: Include timestamp and task ID for traceability

### Summarization Guidelines
- **Decisions**: Technical choices made and their rationale
- **Artifacts**: Files created/modified with brief descriptions
- **Open Questions**: Unresolved items that need attention in later tasks
- **Blockers**: Issues that prevented completion and their resolution status
- **Next Steps**: Recommended actions for subsequent tasks

## Consequences

### Positive
- Enables handling of arbitrarily large features within model context limits
- Preserves critical information across task boundaries
- Reduces token consumption and associated costs
- Improves relevance of context provided to LLMs
- Creates audit trail of feature development progress

### Negative
- Risk of losing important details in summarization process
- Potential for summary hallucination or inaccuracies
- Added complexity in workflow planning and execution
- Need for quality assurance of summary generation

## Implementation Notes
- Use the same LLM for summarization that executes the tasks (self-consistency)
- Implement summarization as a post-task step in the CoderWorkflow
- Allow configuration of summary length and detail level
- Provide mechanism to retrieve full task results when needed for verification
- Consider extractive summarization for critical sections (code snippets, configs)
- Add validation step to ensure summaries capture required information

## Open Questions
1. What is the optimal compression ratio for task summaries (target token reduction)?
2. How should we handle summarization of code changes vs. documentation?
3. What validation mechanisms can ensure summary fidelity?
4. Should we implement different summarization strategies for different task types?
5. How do we balance summary freshness with computational overhead?

## Related Decisions
- Complements persistent workspace strategy (ADR-007) by reducing contextual overhead
- Informs multi-model routing (ADR-006) through context-aware model selection
- Relates to future workspace cleanup strategies by limiting metadata growth