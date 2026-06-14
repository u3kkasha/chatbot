# Git Hooks recipes

# Pre-commit hook command
hook-pre-commit: check-format secret-scanning
    @echo "Pre-commit checks passed."

# Pre-push hook command
hook-pre-push: verify
    @echo "Pre-push checks passed."

# Commit message validation hook command
hook-commit-msg msg_file:
    node .hooks/validate-commit.mjs {{ msg_file }}
