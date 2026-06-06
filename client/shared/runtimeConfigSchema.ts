import { type } from 'arktype'

// Runtime config schema validated by nuxt-safe-runtime-config at build-time and startup.
// Keys mirror .env.local.example and the NUXT_* environment variable convention.
export const runtimeConfigSchema = type({
  // ── Private (server-only) ──────────────────────────────────────────────────
  databaseUrl: 'string',
  redisUrl: 'string',
  blobStorageConnectionString: 'string',
  qdrantApiKey: 'string',

  // ── Public (exposed to client) ─────────────────────────────────────────────
  public: {
    apiBase: 'string'
  }
})
