# ── Chatbot — Justfile ────────────────────────────────────────────────────────
# Requires: just, bun, dotnet, lefthook, git
# Run inside the Nix dev shell (`nix develop`) so PRJ_ROOT is set.

set shell := ["bash", "-euo", "pipefail", "-c"]

# ── Variables ─────────────────────────────────────────────────────────────────

# ── Default: list all recipes ─────────────────────────────────────────────────

default:
    @just --list

# ── Top-level setup (full sequence) ───────────────────────────────────────────

# Full first-time project setup
setup: hooks backend frontend
    @echo ""
    @echo "⚙ Setup complete! Run 'run' to start the platform."


# ── Individual steps ──────────────────────────────────────────────────────────

# Install git hooks via Lefthook
hooks:
    @echo "▶ Installing git hooks (Lefthook)"
    lefthook install
    @echo "✔ Git hooks installed"

# Restore backend dependencies
backend:
    @echo "▶ Restoring backend dependencies (dotnet)"
    dotnet restore
    @echo "✔ Backend dependencies restored"

# Install frontend dependencies
frontend:
    @echo "▶ Installing frontend dependencies (bun)"
    cd "${PRJ_ROOT}/client" && bun install
    @echo "✔ Frontend dependencies installed"

# ── Dev shortcuts ─────────────────────────────────────────────────────────────

# Start the Tilt development environment
run:
    tilt up

# Format all files
fmt:
    treefmt
