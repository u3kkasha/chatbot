#!/usr/bin/env bash
set -euo pipefail

# Ensure we are in a git repository
if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "Error: Not in a git repository." >&2
    exit 1
fi

# Get branch name from argument, or prompt if not provided
BRANCH_NAME="${1:-}"
if [ -z "$BRANCH_NAME" ]; then
    read -p "Enter branch name for the new worktree: " BRANCH_NAME
fi

if [ -z "$BRANCH_NAME" ]; then
    echo "Error: Branch name cannot be empty." >&2
    exit 1
fi

REPO_ROOT=$(git rev-parse --show-toplevel)
PARENT_DIR=$(dirname "$REPO_ROOT")
WORKTREE_PATH="$PARENT_DIR/$BRANCH_NAME"

if [ -d "$WORKTREE_PATH" ]; then
    echo "Error: Directory '$WORKTREE_PATH' already exists." >&2
    exit 1
fi

echo "Fetching latest main from origin..."
git fetch origin main

# Attempt to update local main branch if currently checked out in this worktree
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" = "main" ]; then
    echo "Updating local main branch..."
    git merge --ff-only origin/main || echo "Warning: Could not fast-forward local main branch to origin/main. Proceeding anyway."
fi

# Create the worktree
if git show-ref --verify --quiet "refs/heads/$BRANCH_NAME"; then
    echo "Branch '$BRANCH_NAME' already exists locally. Creating worktree..."
    git worktree add "$WORKTREE_PATH" "$BRANCH_NAME"
else
    echo "Creating new branch '$BRANCH_NAME' from origin/main and adding worktree..."
    git worktree add -b "$BRANCH_NAME" "$WORKTREE_PATH" origin/main
fi

# CD into the worktree by spawning a subshell if standard input is a TTY
if [ -t 0 ]; then
    echo "Entering worktree: $WORKTREE_PATH (type 'exit' to return)"
    cd "$WORKTREE_PATH"
    exec "${SHELL:-bash}"
else
    echo "Worktree created at: $WORKTREE_PATH"
    echo "To enter, run: cd \"$WORKTREE_PATH\""
fi
