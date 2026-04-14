# ADR-007: Persistent Workspace Strategy

## Status
Accepted

## Context
Currently, the Agent Process uses temporary workspaces that are cloned for each workflow execution. This approach provides isolation but incurs performance costs due to repeated repository cloning and dependency installation. For multi-task features where subsequent tasks build upon previous work, this creates unnecessary overhead.

## Decision
Replace temporary workspaces with persistent sandboxes per project using per-project git worktrees. This approach maintains isolation while improving performance by avoiding repeated full clones.

### Implementation Approach
1. **Per-Project Worktrees**: Each project gets a dedicated git worktree that persists across workflow executions
2. **Task-Specific Branches**: Within each worktree, tasks execute in isolated branches that are cleaned up after completion
3. **Shared Dependencies**: Node_modules and .NET packages can be cached at the worktree level
4. **Atomic Updates**: Worktree updates are atomic to prevent corruption during concurrent access

### Benefits
- **Performance**: Eliminates repeated repository cloning and dependency restoration
- **Consistency**: Maintains workspace state between related tasks
- **Efficiency**: Enables incremental builds and faster test execution
- **Isolation**: Each task still operates in its own branch preventing cross-task contamination

### Operational Considerations
- **Cleanup Strategy**: Implement periodic cleanup of stale worktrees and branches
- **Disk Usage**: Monitor and manage disk space usage for persistent worktrees
- **Concurrency**: Handle concurrent access to the same worktree from different tasks
- **Backup**: Consider backup strategy for active worktrees if needed

## Consequences

### Positive
- Significant performance improvement for multi-task workflows
- Reduced network and I/O overhead
- Better developer experience with faster iteration cycles
- Maintains security isolation between unrelated projects

### Negative
- Increased disk space usage persistence
- Need for cleanup mechanisms to prevent unbounded growth
- Added complexity in workspace management
- Potential for workspace corruption if not properly managed

## Implementation Notes
- Use `git worktree add` to create project-specific worktrees
- Create task-specific branches with `git checkout -b task-{taskId}`
- Clean up task branches after completion but retain worktree
- Implement workspace pooling for frequently accessed projects
- Add monitoring for workspace health and performance metrics

## Related Decisions
- Builds on ADR-006 (Multi-model routing) for efficient task execution
- Complements future context management strategies (Step 6.4)