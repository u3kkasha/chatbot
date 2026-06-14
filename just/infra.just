# Infrastructure recipes

# Spin up local infrastructure services
infra-up:
    docker compose up -d

# Shut down local infrastructure services
infra-down:
    docker compose down

# View logs for infrastructure services
infra-logs:
    docker compose logs -f
