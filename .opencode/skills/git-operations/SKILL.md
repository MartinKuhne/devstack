---
name: git-operations
description: Git operations including branch management, commits, and repository maintenance for feature branches
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: feature-development
---

## What I do

I provide guidance and best practices for Git operations, focusing on:
- Creating and managing feature branches
- Making commits with meaningful messages
- Branch naming conventions
- Handling merge conflicts
- Keeping branches up-to-date with main

## Branch Naming Convention

Use this format for feature branches:
```
feature/<ticket-id>-<short-description>
feature/ai-123-add-user-auth
bugfix/456-fix-login-error
hotfix/789-security-patch
```

## Commit Message Format

Follow conventional commits:
```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Example:
```
feat(auth): add OAuth2 authentication support

- Implement Google OAuth2 flow
- Add token refresh logic
- Update user model with provider field

Closes #123
```

## Working with Feature Branches

### Create and Switch
```bash
git checkout -b feature/123-add-auth
```

### Update from Main
```bash
git fetch origin
git rebase origin/main
```

### Commit Changes
```bash
git add <files>
git commit -m "feat(scope): description"
```

### Push Branch
```bash
git push -u origin feature/123-add-auth
```

## Merge Conflict Resolution

1. Identify conflicting files:
```bash
git status
```

2. Open conflicting files and resolve marked sections:
```<<<<<<< HEAD
your changes
=======
incoming changes
>>>>>>> branch-name
```

3. After resolving, stage and commit:
```bash
git add <resolved-files>
git commit
```

## Best Practices

- Commit early and often with clear messages
- Keep commits atomic (one logical change per commit)
- Rebase over merge when updating from main
- Use `--force-with-lease` when force pushing (never `--force`)
- Delete merged branches after PR approval
- Always pull latest main before creating new feature branches
