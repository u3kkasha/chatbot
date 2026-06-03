# Tiltfile for Chatbot Project

# Infrastructure
docker_compose('docker-compose.yml')

# Frontend (Nuxt 4)
local_resource(
    'frontend',
    serve_cmd='cd client && bun run dev',
    deps=['client'],
    ignore=['client/node_modules', 'client/.nuxt'],
    resource_deps=['api'],
    readiness_probe=probe(
        http_get=http_get_action(port=3000),
        initial_delay_secs=5,
        period_secs=2
    )
)

# Backend (.NET 10 API with MAF)
local_resource(
    'api',
    serve_cmd='cd api && dotnet watch run',
    deps=['api'],
    ignore=['api/bin', 'api/obj'],
    resource_deps=['postgres', 'qdrant', 'azurite', 'docling-serve'],
    readiness_probe=probe(
        http_get=http_get_action(port=5136, path='/health'),
        initial_delay_secs=2,
        period_secs=2
    )
)
