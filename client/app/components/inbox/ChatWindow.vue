<script setup lang="ts">
export interface ChatMessage {
  id: string;
  role: "customer" | "agent";
  text: string;
  timestamp?: string;
}

export interface AiSuggestion {
  id: string;
  text: string;
}

defineProps<{
  customerName: string;
  isPriority?: boolean;
  messages: ChatMessage[];
  suggestions: AiSuggestion[];
}>();

const emit = defineEmits<{
  send: [text: string];
  useSuggestion: [text: string];
}>();

const draft = ref("");

function send() {
  const text = draft.value.trim();
  if (!text) return;
  emit("send", text);
  draft.value = "";
}

function onSuggestion(suggestion: AiSuggestion) {
  draft.value = suggestion.text;
  emit("useSuggestion", suggestion.text);
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
    send();
  }
}
</script>

<template>
  <section class="flex-1 flex flex-col bg-[var(--color-background)] min-w-0">
    <!-- Chat header -->
    <div
      class="h-16 px-6 border-b border-[var(--color-outline-variant)] flex items-center justify-between bg-[var(--color-surface-container-lowest)] shrink-0"
    >
      <div class="flex items-center gap-3">
        <!-- Mobile back button -->
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-arrow-back"
          class="lg:hidden rounded-full -ml-2"
          aria-label="Back to conversations"
        />
        <h2 class="text-lg font-semibold text-[var(--color-on-surface)]">
          {{ customerName }}
        </h2>
        <span
          v-if="isPriority"
          class="px-2 py-0.5 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded text-[11px] font-semibold border border-[#ffb4ab]"
        >
          High Priority
        </span>
      </div>

      <div class="flex items-center gap-1">
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-check-circle-outline"
          class="rounded-full"
          title="Resolve conversation"
          aria-label="Resolve"
        />
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-move-up"
          class="rounded-full"
          title="Transfer conversation"
          aria-label="Transfer"
        />
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-info-outline"
          class="rounded-full xl:hidden"
          title="Customer info"
          aria-label="Customer info"
        />
      </div>
    </div>

    <!-- Messages area -->
    <div class="flex-1 overflow-y-auto p-6 space-y-6 flex flex-col">
      <template v-for="(msg, idx) in messages" :key="msg.id">
        <!-- Date separator (shown before first message only for demo) -->
        <div v-if="idx === 0" class="text-center">
          <span
            class="text-[11px] text-[var(--color-secondary)] bg-[var(--color-surface-container)] px-3 py-1 rounded-full"
          >
            Today
          </span>
        </div>

        <!-- Customer bubble -->
        <div
          v-if="msg.role === 'customer'"
          class="flex items-end gap-3 max-w-[80%]"
        >
          <div
            class="w-8 h-8 rounded-full bg-[var(--color-surface-variant)] text-[var(--color-primary)] flex items-center justify-center text-xs font-bold shrink-0"
          >
            {{ customerName.split(" ").map((n) => n[0]).join("").slice(0, 2) }}
          </div>
          <div
            class="bg-[var(--color-surface-container-low)] px-4 py-3 rounded-2xl rounded-bl-sm text-[var(--color-on-surface)] text-sm leading-relaxed ambient-shadow"
          >
            {{ msg.text }}
          </div>
        </div>

        <!-- Agent bubble -->
        <div
          v-else
          class="flex items-end gap-3 max-w-[80%] self-end flex-row-reverse"
        >
          <div
            class="w-8 h-8 rounded-full bg-[var(--color-primary)] text-white flex items-center justify-center text-xs font-bold shrink-0"
          >
            AM
          </div>
          <div
            class="bg-[var(--color-primary)] px-4 py-3 rounded-2xl rounded-br-sm text-white text-sm leading-relaxed ambient-shadow"
          >
            {{ msg.text }}
          </div>
        </div>
      </template>
    </div>

    <!-- AI Suggestions + Input -->
    <div
      class="p-4 bg-[var(--color-surface-container-lowest)] border-t border-[var(--color-outline-variant)] shrink-0"
    >
      <!-- Magic Reply suggestions -->
      <div v-if="suggestions.length" class="mb-3">
        <div class="flex items-center gap-1.5 mb-2">
          <UIcon
            name="i-material-symbols-auto-awesome"
            class="text-[var(--color-primary)] text-base"
          />
          <span
            class="text-[11px] font-semibold text-[var(--color-primary)] uppercase tracking-wider"
          >
            Magic Reply Suggestions
          </span>
        </div>
        <div class="flex gap-2 overflow-x-auto pb-2 no-scrollbar">
          <button
            v-for="s in suggestions"
            :key="s.id"
            class="shrink-0 bg-blue-50 border border-[var(--color-primary-fixed-dim)] hover:border-[var(--color-primary)] hover:bg-blue-100 px-3 py-2 rounded-lg text-xs text-[var(--color-on-surface)] text-left transition-all duration-150 max-w-[250px] truncate focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
            :title="s.text"
            @click="onSuggestion(s)"
          >
            {{ s.text }}
          </button>
        </div>
      </div>

      <!-- Rich text input -->
      <div
        class="border border-[var(--color-outline-variant)] rounded-xl bg-[var(--color-surface)] focus-within:ring-2 focus-within:ring-[var(--color-primary)] focus-within:ring-offset-1 focus-within:border-transparent transition-all ambient-shadow flex flex-col"
      >
        <textarea
          v-model="draft"
          placeholder="Type your message…  (Ctrl+Enter to send)"
          class="w-full bg-transparent border-none resize-none p-3 focus:ring-0 text-sm placeholder:text-[var(--color-secondary)] min-h-[80px] outline-none"
          @keydown="onKeydown"
        />
        <div
          class="flex items-center justify-between p-2 border-t border-[var(--color-surface-dim)]"
        >
          <div class="flex items-center gap-1 text-[var(--color-secondary)]">
            <UButton
              color="neutral"
              variant="ghost"
              icon="i-material-symbols-format-bold"
              size="xs"
              class="rounded"
              aria-label="Bold"
            />
            <UButton
              color="neutral"
              variant="ghost"
              icon="i-material-symbols-format-italic"
              size="xs"
              class="rounded"
              aria-label="Italic"
            />
            <UButton
              color="neutral"
              variant="ghost"
              icon="i-material-symbols-attach-file"
              size="xs"
              class="rounded"
              aria-label="Attach file"
            />
          </div>
          <UButton
            id="send-message-btn"
            icon="i-material-symbols-send"
            trailing
            size="sm"
            class="rounded-lg"
            aria-label="Send message"
            @click="send"
          >
            Send
          </UButton>
        </div>
      </div>
    </div>
  </section>
</template>
