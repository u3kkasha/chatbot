# Git Workflow Strategy

This document defines the git standards for the Chatbot project. All contributors (human and AI) must adhere to these rules.

## Commit Message Format

We follow **Conventional Commits** combined with **Abstract Gitmojis**.

**Format:** `<emoji> <type>(<scope>): <description>`

### Types & Emojis

Only use **abstract** symbols. Never use "animate" beings (bugs, rockets, people, etc.).

| Type       | Emoji | Description                                                    |
| :--------- | :---- | :------------------------------------------------------------- |
| `feat`     | 📦    | New features or domain logic.                                  |
| `fix`      | 🔧    | Bug fixes (Note: Never use the bug emoji 🐛).                  |
| `perf`     | ⚡    | Performance improvements (Native AOT, caching).                |
| `refactor` | ♻️    | Code changes that neither fix a bug nor add a feature.         |
| `test`     | 🧪    | Adding or correcting tests.                                    |
| `docs`     | 📝    | Documentation only changes.                                    |
| `style`    | 🎨    | Changes that do not affect code meaning (linting, formatting). |
| `chore`    | ⚙️    | Build process, auxiliary tools, or config.                     |

**Example:** `📦 feat(chat): implement message persistence in storage broker`

## Development Lifecycle

### 1. The TDD Heartbeat

Commits should be frequent and mirror the **Red-Green-Refactor** cycle:

- **🧪 test(scope): [failing test description]**: After writing a failing test.
- **📦 feat(scope): [implementation description]**: Once the test passes.
- **♻️ refactor(scope): [optimization description]**: After cleaning up the implementation.

### 2. Definition of Done for Commits

A commit is ready only if:

1. **Surgical:** It addresses exactly one logical change.
2. **Formatted:** `treefmt` has been run.
3. **Tested:** The specific tests related to the change pass.
4. **Compile:** The code compiles (verified for Native AOT readiness).

## Merging and History

- **Branching & Worktrees:** We use the sibling git worktrees method for development. Use descriptive branch names: `<type>/<short-description>` (e.g., `feat/chat-persistence`).
- **Rebase Policy:** Always rebase your feature branch on top of `main` before merging to maintain a linear history.
- **Squash on Merge:** When merging into `main`, squash your development commits into high-quality, atomic commits that represent the "completed" unit of work.

## Sibling Git Worktrees Strategy

The repository is structured as a bare repository at the root directory, with the `main` branch checked out in `main/`. All development branches must be created as sibling git worktrees instead of checking out different branches directly inside the `main/` directory.

### Benefits of Sibling Worktrees

1. **Pristine Main:** Keeps the `main/` directory pristine and free from build artifacts or dependencies of active feature branches.
2. **Isolation:** Isolates build artifacts (`bin`, `obj`), packages, and environment configurations between branches.
3. **Concurrency:** Allows developers or agents to work on multiple features or tasks simultaneously in isolated directories.

### Creating a Worktree

To create a new sibling worktree for a branch:

```bash
# Run from within the main directory:
git worktree add ../<folder-name> -b <type>/<branch-name>
```

Example:

```bash
git worktree add ../chat-persistence -b feat/chat-persistence
```

### Removing a Worktree

To clean up and remove a sibling worktree after your branch is merged:

```bash
# Run from within the main directory:
git worktree remove ../chat-persistence
git worktree prune
```

## AI-Specific Merging Procedures

When an AI agent is asked to "wrap up" or "merge" a feature:

1. **Identify Range:** Use `git log main..HEAD --oneline` to identify all commits made in the feature branch.
2. **Review Changes:** Use `git diff main..HEAD` to understand the total impact.
3. **Verify Compliance:** Ensure all changes meet the "Definition of Done" (formatted, tested).
4. **Execute Squash:** Prefer using `git merge --squash <branch>` or manual squashing to ensure `main` receives a single, high-signal commit with the correct `<emoji> <type>(<scope>): <message>` format.

## Enforcement

- **Husky:** pre-commit and pre-push hooks enforce linting and testing.
- **Gitmojis:** Strict "Abstract Only" rule is a project mandate.
