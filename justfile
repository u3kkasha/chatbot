# Justfile for Chatbot Project

set shell := ["bash", "-c"]

# --- Orchestration ---

# Initial setup: install dependencies and start infrastructure
setup: init-env server-restore client-install infra-up
    @echo "Waiting for database to be ready..."
    node -e "setTimeout(() => {}, 5000);"
    just server-migrate
    lefthook install
    @echo "Setup complete. You can now run 'run' to start the development environment."

# Initialize local environment file (Cross-platform)
init-env:
    node .hooks/init-env.mjs

# Start infrastructure and show instructions
run: infra-up
    @echo \"Infrastructure is up.\"
    @echo \"To start the backend:  just server-run\"
    @echo \"To start the frontend: just client-run\"
    @echo \"Logs:                  just infra-logs\"

# Development mode: start infrastructure and both services in parallel
dev: infra-up
    @echo \"Starting backend and frontend in parallel...\"
    just --parallel server-run client-run

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

# Apply all database migrations
server-migrate:
    @echo "Applying Chat migrations..."
    dotnet ef database update --context Chatbot.Modules.Chat.Brokers.Storage.StorageBroker --project src/Modules/Chat/Chatbot.Modules.Chat.csproj --startup-project api/Chatbot.Api.csproj
    @echo "Applying Identity migrations..."
    dotnet ef database update --context Chatbot.Modules.Identity.Brokers.Storage.StorageBroker --project src/Modules/Identity/Chatbot.Modules.Identity.csproj --startup-project api/Chatbot.Api.csproj
    @echo "Applying Knowledge migrations..."
    dotnet ef database update --context Chatbot.Modules.Knowledge.Brokers.Storage.StorageBroker --project src/Modules/Knowledge/Chatbot.Modules.Knowledge.csproj --startup-project api/Chatbot.Api.csproj

# Generate initial migrations for all modules
server-migrations-add name="InitialCreate":
    @echo "Generating Chat migrations..."
    dotnet ef migrations add {{ name }} --context Chatbot.Modules.Chat.Brokers.Storage.StorageBroker --project src/Modules/Chat/Chatbot.Modules.Chat.csproj --startup-project api/Chatbot.Api.csproj
    @echo "Generating Identity migrations..."
    dotnet ef migrations add {{ name }} --context Chatbot.Modules.Identity.Brokers.Storage.StorageBroker --project src/Modules/Identity/Chatbot.Modules.Identity.csproj --startup-project api/Chatbot.Api.csproj
    @echo "Generating Knowledge migrations..."
    dotnet ef migrations add {{ name }} --context Chatbot.Modules.Knowledge.Brokers.Storage.StorageBroker --project src/Modules/Knowledge/Chatbot.Modules.Knowledge.csproj --startup-project api/Chatbot.Api.csproj

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

# Generate OpenAPI document
server-openapi-gen:
    node .hooks/generate-openapi.mjs

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
client-gen: server-openapi-gen
    bun run --cwd client openapi:generate

# --- Quality & Formatting ---

# Run unified formatting for the entire project
format: format-server format-client format-nix format-just format-docs format-cue

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

# Format Justfile
format-just:
    just --fmt

# Format documentation and config files (Markdown, YAML, JSON)
format-docs:
    bunx prettier --write "**/*.{md,yaml,yml,json}"

# Format CUE files
format-cue:
    cue fmt ./...

# Verify formatting for the entire project without modifying files
check-format: check-server check-client check-nix check-just check-docs check-cue

# Check backend formatting
check-server:
    dotnet format whitespace --verify-no-changes
    dotnet format style --verify-no-changes

# Check frontend formatting
check-client:
    @if [ -d client/node_modules ]; then bun run --cwd client lint; else echo "Skipping client lint (node_modules missing)"; fi

# Check Nix formatting
check-nix:
    alejandra --check .

# Check Justfile formatting
check-just:
    just --fmt --check

# Check documentation formatting
check-docs:
    bunx prettier --check "**/*.{md,yaml,yml,json}"

# Check CUE formatting
check-cue:
    cue fmt --check ./...

# Check if API generation creates any changes
check-api-drift: client-gen
    node .hooks/check-api-drift.mjs

# Run secret scanning
secret-scanning:
    gitleaks protect --staged --verbose
# Unified quality check (CI equivalent)
ci: verify server-test client-test

# Run only live integration tests (requires AI__ApiKey)
test-live:
    dotnet test tests/Chatbot.Tests.Integration/Chatbot.Tests.Integration.csproj --filter "Category=Live"

# Fast verification (build, lint, types) - used for local pre-push
verify: check-format check-api-drift client-typecheck client-knip server-build

# --- Git Hooks ---

# Pre-commit hook command
hook-pre-commit: check-format secret-scanning
    @echo \"Pre-commit checks passed.\"

# Pre-push hook command
hook-pre-push: verify
    @echo \"Pre-push checks passed.\"

# Commit message validation hook command
hook-commit-msg msg_file:
    node .hooks/validate-commit.mjs {{ msg_file }}
