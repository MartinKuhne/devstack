---
name: gitea-pr
description: Create and manage pull requests on Gitea using the REST API for code review workflow
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: code-review
---

## What I do

I provide guidance for creating and managing pull requests on Gitea, including:
- Creating pull requests with proper descriptions
- Updating PR status
- Adding reviewers
- Managing PR labels
- Handling PR reviews

## Gitea API Reference

Base URL: `{GITEA_URL}/api/v1`

Authentication: `Authorization: token {GITEA_API_TOKEN}`

### Create Pull Request

```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "feat(auth): add OAuth2 support",
    "body": "## Summary\n\nThis PR adds OAuth2 authentication.\n\n## Changes\n- Add Google OAuth2 provider\n- Implement token refresh",
    "head": "feature/123-oauth2",
    "base": "main"
  }'
```

### Get Pull Request

```bash
curl "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}" \
  -H "Authorization: token {GITEA_API_TOKEN}"
```

### List Pull Requests

```bash
curl "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls?state=open&limit=50" \
  -H "Authorization: token {GITEA_API_TOKEN}"
```

### Update Pull Request

```bash
curl -X PATCH "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated title",
    "body": "Updated description"
  }'
```

### Add Reviewers

```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}/requested_reviewers" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"reviewers": ["username1", "username2"]}'
```

### Merge Pull Request

```bash
curl -X POST "{GITEA_URL}/api/v1/repos/{owner}/{repo}/pulls/{pr_number}/merge" \
  -H "Authorization: token {GITEA_API_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"merge_when_all_successful": true}'
```

## Pull Request Template

Use this structure for PR descriptions:

```markdown
## Summary

Brief description of changes (1-3 sentences).

## Type of Change

- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Changes

- Change 1
- Change 2
- Change 3

## Testing

Describe testing performed.

## Screenshots (if applicable)

## Related Issues

Closes #123
Fixes #456

## Checklist

- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Tests added/updated
- [ ] Documentation updated
```

## Workflow

1. **Create branch** from main with feature name
2. **Implement changes** with atomic commits
3. **Push branch** and open PR with template
4. **Request review** from team members
5. **Address feedback** with follow-up commits
6. **Merge** once approved

## Environment Variables

- `GITEA_URL`: Gitea server URL (e.g., http://localhost:3002)
- `GITEA_API_TOKEN`: API token for authentication
- `GITEA_DEFAULT_BRANCH`: Target branch for PRs (default: main)

## Python Integration

The codebase provides `GiteaClient` in `src/services/gitea_client.py`:

```python
from src.services.gitea_client import GiteaClient
from src.config import load_config

config = load_config()
async with GiteaClient(config) as gitea:
    pr = await gitea.create_pull_request(
        owner="myorg",
        repo="myrepo",
        title="feat: add feature",
        body="PR description",
        head="feature/branch",
        base="main"
    )
```

## Common Issues

- **Branch not found**: Ensure branch exists and is pushed
- **Merge conflict**: Rebase feature branch on target
- **Permission denied**: Check API token has repo access
- **Rate limiting**: Implement retry logic with backoff
