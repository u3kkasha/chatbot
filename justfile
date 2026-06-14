# Justfile for Chatbot Project

set shell := ["bash", "-c"]

import 'just/infra.just'
import 'just/server.just'
import 'just/client.just'
import 'just/quality.just'
import 'just/hooks.just'

# --- Orchestration ---

# Initial setup: install dependencies and start infrastructure
setup: init-env server-restore client-install infra-up
    @echo "Waiting for database to be ready..."
    node -e "setTimeout(() => {}, 5000);"
    just server-migrate
    just server-seed
    lefthook install
    @echo "Setup complete. You can now run 'run' to start the development environment."

# Initialize local environment file (Cross-platform)
init-env:
    node .hooks/init-env.mjs

# Start infrastructure, then server, then client
run: infra-up
    @echo "Starting backend..."
    just server-run & \
    for i in {1..30}; do \
        if curl -s -k http://localhost:5136/health >/dev/null || curl -s -k https://localhost:7214/health >/dev/null; then \
            break; \
        fi; \
        sleep 1; \
    done; \
    echo "Backend is ready! Starting frontend..."; \
    just client-run & \
    wait
