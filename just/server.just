# Server recipes (.NET 10 API)

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

# Seed the database with demo data
server-seed:
    @echo "Seeding database with demo data..."
    dotnet run --project api/Chatbot.Api.csproj -- --seed

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
