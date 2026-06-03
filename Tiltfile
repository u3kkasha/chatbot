# Tiltfile for Chatbot Project

# Infrastructure
docker_compose('docker-compose.yml')

# Frontend (Nuxt 4)
local_resource(
    'frontend',
    cmd='cd client && bun run dev',
    deps=['client'],
    ignore=['client/node_modules']
)

# Backend (.NET 10 API with MAF)
local_resource(
    'api',
    cmd='cd api && dotnet watch run',
    deps=['api'],
    ignore=['api/bin', 'api/obj']
)
