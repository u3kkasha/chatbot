---
name: Chatbot Frontend
description: Enforces Nuxt 4 Client conventions, Pinia Colada for state/query management, Tailwind CSS/Nuxt UI sorting, and automated type/schema generation from OpenAPI.
---

# Chatbot Frontend Guidelines

This skill governs the frontend architectural decisions, directory structures, and UI/UX design rules for the Nuxt 4 client application.

## ⚙️ Nuxt 4 & Tech Stack Conventions

- **Core:** Nuxt 4, Vue 3, TypeScript. Use the Composition API with `<script setup>` syntax.
- **UI Components:** **Nuxt UI** and Tailwind CSS.
- **State Management:** **Pinia** for local/global UI state.
- **Data Fetching & Caching:** **Pinia Colada** (`@pinia/colada`) for queries, mutations, and automatic cache management.

---

## ⚡ API Client & Validation Schema Generation

To ensure backend and frontend models are perfectly synchronized, we auto-generate our API client, TypeScript types, and validation schemas from the backend OpenAPI JSON using `@hey-api/openapi-ts`.

### 1. Generation Pipeline
- The backend builds and outputs `openapi.json`.
- The frontend runs `npx openapi-ts` to generate type definitions, HTTP fetch methods (using `@hey-api/client-ofetch`), and **Valibot** validation schemas.
- Do not manually code API requests, DTO TypeScript interfaces, or form validations. Always import from the generated client.

### 2. Form Validation
- Use the auto-generated **Valibot** schemas to validate forms before submitting them to the backend API.

---

## 🔧 Design and Aesthetics Mandates

- **Modern Visuals:** Use vibrant, cohesive HSL-tailored color palettes (sleek dark mode default, subtle gradients, glassmorphism).
- **Typography:** Avoid default browser fonts. Use high-quality Google Fonts (e.g., Inter, Outfit, Roboto).
- **Micro-animations:** Implement smooth CSS transitions and hover states for all interactive elements to make the UI feel alive.
- **Inbox Interface:** The primary operator dashboard uses a 3-pane layout:
  1. **Left Pane:** Chat session lists (filtered by status and tenant).
  2. **Middle Pane:** Active chat conversation thread with real-time updates and Server-Sent Events (SSE) AI suggestions.
  3. **Right Pane:** Contextual customer profile and knowledge base search.

---

## 🔒 Content Guidelines & Safety (Animate Beings Restriction)

- **Strict Constraint:** **NEVER** use images, icons, or emojis of animate beings (people, animals, bugs, robots, etc.) in the frontend user interface, layout icons, or mock data.
- **Allowed Alternatives:** Use abstract symbols, geometric shapes, or plain text.
  - *Prohibited:* 👤 (user icon), 🤖 (robot icon), 🐛 (bug), 🚀 (rocket), 🐳 (docker).
  - *Compliant:* 🔧 (wrench), ⚙️ (gear), ⚡ (bolt), 📦 (package), 🔒 (lock), key, chat box, checkmarks, line graphs.
- If a user profile picture or placeholder is needed, generate an abstract geometric pattern (like a letter avatar with a stylized background) rather than a silhouette or person icon.
