---
name: code-review
description: Perform thorough code reviews including security checks, best practices, and constructive feedback
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: quality-assurance
---

## What I do

I perform comprehensive code reviews with focus on:
- Code quality and maintainability
- Security vulnerabilities
- Performance issues
- Best practices adherence
- Test coverage
- Documentation completeness

## Review Checklist

### 1. Correctness
- [ ] Does the code do what the PR claims?
- [ ] Are edge cases handled?
- [ ] Are there potential bugs?
- [ ] Are error conditions properly handled?

### 2. Security
- [ ] Input validation present?
- [ ] SQL injection prevention?
- [ ] XSS prevention?
- [ ] Authentication/authorization correct?
- [ ] Secrets not exposed in code?
- [ ] Proper error messages (no info leakage)?

### 3. Performance
- [ ] N+1 query issues?
- [ ] Unnecessary iterations?
- [ ] Large data in memory?
- [ ] Caching opportunities?

### 4. Code Style
- [ ] Follows project conventions?
- [ ] Naming conventions consistent?
- [ ] Code duplication minimized?
- [ ] Functions single-purpose?

### 5. Testing
- [ ] Unit tests for new logic?
- [ ] Integration tests for flows?
- [ ] Edge cases covered?
- [ ] Test readability?

### 6. Documentation
- [ ] Comments for complex logic?
- [ ] API documentation updated?
- [ ] README updated if needed?
- [ ] Breaking changes documented?

## Review Comment Format

Use structured comments for clarity:

### For Issues
```
[Issue]: Brief description
[Severity]: High/Medium/Low
[Suggestion]: Suggested fix or approach
```

Example:
```
[Issue]: SQL query vulnerable to injection
[Severity]: High
[Suggestion]: Use parameterized query:
  db.query("SELECT * FROM users WHERE id = ?", [userId])
```

### For Suggestions
```
[Suggestion]: Improvement idea
[Benefit]: What this improves
[Effort]: Low/Medium/High
```

### For Praise
```
[Nit]: Minor style preference (optional to address)
[Good]: Specific positive observation
```

## Review Workflow

1. **Understand context** - Read PR description and linked issues
2. **Check scope** - Ensure changes match stated goals
3. **Review iteratively** - Top-down: architecture, logic, style
4. **Test locally** - Pull and verify functionality
5. **Provide feedback** - Clear, actionable, constructive
6. **Follow up** - Verify fixes addressed concerns

## Gitea Review Actions

### Approve PR
```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}/reviews" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"event": "APPROVE"}'
```

### Request Changes
```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}/reviews" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "event": "REQUEST_REVIEW",
    "body": "Please address the following issues..."
  }'
```

### Comment on PR
```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}/reviews" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"event": "COMMENT", "body": "General comment"}'
```

## Code Analysis Focus Areas

### Python
- Type hints present?
- Async/await used correctly?
- Exception handling proper?
- Memory efficiency?
- Dependency injection?

### JavaScript/TypeScript
- Type safety?
- Async patterns correct?
- Error boundaries?
- Memory leaks?
- Bundle size impact?

### SQL
- Indexes used?
- Query optimization?
- Transaction handling?
- SQL injection prevention?

## Giving Constructive Feedback

DO:
- Be specific and actionable
- Explain the "why" behind concerns
- Offer alternatives
- Acknowledge good work
- Focus on the code, not the coder

DON'T:
- Use absolute language ("always", "never")
- Nitpick style over substance
- Make personal comments
- Block on minor preferences
- Forget to approve when satisfied

## Priority Guidelines

### Blocking (Must Fix)
- Security vulnerabilities
- Breaking bugs
- Data corruption risks
- Major architectural issues

### Important (Should Fix)
- Performance issues
- Missing tests
- Unclear code
- Incomplete error handling

### Nice to Have (Consider)
- Code style preferences
- Refactoring suggestions
- Alternative approaches
- Documentation improvements
