package main

import (
	"tool/os"
)

#Env: {
	// Connection strings
	ConnectionStrings__DefaultConnection?: string & =~"^Host=.+"
	ConnectionStrings__Redis?:             string & =~"^.+"
	ConnectionStrings__BlobStorage?:       string & =~"^DefaultEndpointsProtocol=.+"

	// Qdrant
	Qdrant__Host?:     string & =~"^.+"
	Qdrant__Port?:     string & =~"^[0-9]+$"
	Qdrant__UseHttps?: "true" | "false"
	Qdrant__ApiKey?:   string

	// AI Provider
	AI__Endpoint?:   string & =~"^https?://.+"
	AI__ApiKey?: string & =~"^.+"
	AI__ModelId?:    string & =~"^.+"
	AI__EmbeddingModelId?: string & =~"^.+"
	AI__RerankModelId?:    string & =~"^.+"

	// Docling-serve
	Processing__BaseUrl?: string & =~"^https?://.+"

	// MassTransit
	MassTransit__Outbox__Provider?: "Postgres" | "None"

	// Seq
	Serilog__WriteTo__0__Name?:            "Seq"
	Serilog__WriteTo__0__Args__serverUrl?: string & =~"^https?://.+"
	Serilog__WriteTo__0__Args__apiKey?:    string

	// Nuxt / Frontend
	NUXT_DATABASE_URL?:                   string & =~"^postgresql://.+"
	NUXT_REDIS_URL?:                       string & =~"^redis://.+"
	NUXT_BLOB_STORAGE_CONNECTION_STRING?:  string & =~"^DefaultEndpointsProtocol=.+"
	NUXT_QDRANT_API_KEY?:                  string & =~"^.+"
	NUXT_PUBLIC_API_BASE?:                 string & =~"^https?://.+"

	// Infrastructure Docker Compose Secrets
	POSTGRES_PASSWORD?:              string & =~"^.+"
	SEQ_FIRSTRUN_ADMINPASSWORDHASH?: string & =~"^.+"
	SEQ_API_KEY?:                    string & =~"^.+"

	... // Allow and ignore other environment variables in the shell
}

command: validate: {
	getEnv: os.Environ

	// Unify the environment variables with the open struct schema
	check: #Env & getEnv
}
