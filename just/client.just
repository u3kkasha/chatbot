# Client recipes (Nuxt 4)

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
