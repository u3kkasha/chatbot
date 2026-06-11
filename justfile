# Justfile for Chatbot Project

set shell := ["bash", "-c"]

# --- Orchestration ---

# Initial setup: install dependencies and start infrastructure
setup: server-restore client-install infra-up
    lefthook install
    @echo "Setup complete. You can now run 'run' to start the development environment."

# Start infrastructure and show instructions
run: infra-up
    @echo "Infrastructure is up."
    @echo "To start the backend:  just server-run"
    @echo "To start the frontend: just client-run"
    @echo "Logs:                  just infra-logs"

# --- Infrastructure ---

# Spin up local infrastructure services
infra-up:
    docker compose up -d

# Shut down local infrastructure services
infra-down:
    docker compose down

# View logs for infrastructure services
infra-logs:
    docker compose logs -f

# --- Server (.NET 10 API) ---

# Restore backend dependencies
server-restore:
    dotnet restore

# Run the API in watch mode
server-run:
    dotnet watch --project api/Chatbot.Api.csproj run

# Build the API
server-build:
    dotnet build api/Chatbot.Api.csproj

# Run backend tests
server-test:
    dotnet test

# Run backend linting/formatting check
server-lint:
    dotnet format --verify-no-changes

# --- Client (Nuxt 4) ---

# Install client dependencies
client-install:
    bun install --cwd client

# Run the client in development mode
client-run:
    bun run --cwd client dev

# Build the client
client-build:
    bun run --cwd client build

# Run frontend tests
client-test:
    bun run --cwd client test run

# Run frontend linting
client-lint:
    bun run --cwd client lint

# Run frontend type checking
client-typecheck:
    bun run --cwd client typecheck

# Run frontend knip check
client-knip:
    bun run --cwd client knip

# Generate API client from OpenAPI spec
client-gen:
    bun run --cwd client openapi:generate

# --- Quality & Formatting ---

# Run unified formatting for the entire project
format: format-server format-client format-nix

# Format C# files (whitespace and style)
format-server:
    dotnet format whitespace
    dotnet format style

# Format frontend files using ESLint
format-client:
    bun run --cwd client lint --fix

# Format Nix files using Alejandra
format-nix:
    alejandra .

# Run secret scanning
secret-scanning:
    gitleaks protect --staged --verbose

# --- Git Hooks ---

# Pre-commit hook command
hook-pre-commit: format secret-scanning
    @echo "Pre-commit checks passed."

# Pre-push hook command
hook-pre-push: server-build client-typecheck client-knip server-test client-test
    @echo "Pre-push checks passed."

# Commit message validation hook command
hook-commit-msg msg_file:
    bash .hooks/validate-commit.sh {{msg_file}}
