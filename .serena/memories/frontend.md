# Frontend Development Standards

## Technology Stack

- **Nuxt 4 Client:** TypeScript, Vue 3, TailwindCSS, Nuxt UI.
- **State Management:** Pinia and Pinia Colada (for caching and data fetching state).
- **Zod Validation:** TypeScript types and Valibot/Zod schemas auto-generated from OpenAPI.

## Layout & Real-time Integration

- Unified inbox: 3-pane layout built using native Nuxt UI components.
- Real-time sync: `@microsoft/signalr` client used to process notifications and inbox states.
- Token streaming: SSE (Server-Sent Events) listener for unidirectional token streaming.
- Dependency management: ALWAYS use CLI `bun add <Package>` instead of editing package.json manually.
