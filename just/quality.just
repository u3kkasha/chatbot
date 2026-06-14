# Quality & Formatting recipes

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
