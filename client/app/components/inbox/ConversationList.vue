<script setup lang="ts">
export interface ConversationItem {
  id: string;
  name: string;
  initials: string;
  channel: "live-chat" | "whatsapp" | "email";
  snippet: string;
  timestamp: string;
  isActive?: boolean;
}

defineProps<{
  conversations: ConversationItem[];
  activeId: string;
}>();

const emit = defineEmits<{
  select: [id: string];
}>();

const channelMeta: Record<
  ConversationItem["channel"],
  { label: string; icon: string; badgeClass: string }
> = {
  "live-chat": {
    label: "Live Chat",
    icon: "i-material-symbols-forum-outline",
    badgeClass:
      "bg-[var(--color-surface-container)] text-[var(--color-secondary)]",
  },
  whatsapp: {
    label: "WhatsApp",
    icon: "i-material-symbols-chat-outline",
    badgeClass: "bg-[#e2f5ec] text-[#0d7a46]",
  },
  email: {
    label: "Email",
    icon: "i-material-symbols-mail-outline",
    badgeClass:
      "bg-[var(--color-surface-container-high)] text-[var(--color-secondary)]",
  },
};
</script>

<template>
  <aside
    class="w-[350px] bg-[var(--color-surface-bright)] border-r border-[var(--color-outline-variant)] flex flex-col hidden lg:flex shrink-0"
  >
    <!-- Header -->
    <div
      class="p-4 border-b border-[var(--color-outline-variant)] flex justify-between items-center bg-[var(--color-surface-container-lowest)]"
    >
      <h3
        class="text-[11px] font-semibold text-[var(--color-secondary)] uppercase tracking-wider"
      >
        Active Conversations
      </h3>
      <UButton
        color="neutral"
        variant="ghost"
        icon="i-material-symbols-filter-list"
        size="xs"
        aria-label="Filter conversations"
      />
    </div>

    <!-- List -->
    <div class="flex-1 overflow-y-auto p-2 space-y-1 bg-[var(--color-background)]">
      <div
        v-for="conv in conversations"
        :key="conv.id"
        class="rounded-lg p-3 cursor-pointer relative overflow-hidden flex flex-col gap-2 transition-all duration-200"
        :class="
          conv.id === activeId
            ? 'bg-[var(--color-surface-container-lowest)] border border-[var(--color-outline-variant)] ambient-shadow'
            : 'bg-[var(--color-surface)] border border-transparent hover:border-[var(--color-outline-variant)]'
        "
        @click="emit('select', conv.id)"
      >
        <!-- Active accent bar -->
        <div
          v-if="conv.id === activeId"
          class="absolute left-0 top-0 bottom-0 w-1 bg-[var(--color-primary)] rounded-l-lg"
        />

        <div class="flex justify-between items-start">
          <div class="flex items-center gap-3">
            <!-- Letter avatar -->
            <div
              class="w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold shrink-0 ambient-shadow"
              :class="
                conv.id === activeId
                  ? 'bg-[var(--color-primary)] text-white'
                  : 'bg-[var(--color-surface-variant)] text-[var(--color-primary)]'
              "
            >
              {{ conv.initials }}
            </div>

            <div>
              <h4
                class="text-[15px] font-semibold"
                :class="
                  conv.id === activeId
                    ? 'text-[var(--color-on-surface)]'
                    : 'text-[var(--color-secondary)]'
                "
              >
                {{ conv.name }}
              </h4>
              <div class="flex items-center gap-1 mt-0.5">
                <span
                  class="px-1.5 py-0.5 rounded text-[11px] font-medium flex items-center gap-1"
                  :class="channelMeta[conv.channel].badgeClass"
                >
                  <UIcon :name="channelMeta[conv.channel].icon" class="text-xs" />
                  {{ channelMeta[conv.channel].label }}
                </span>
              </div>
            </div>
          </div>

          <span class="text-[11px] text-[var(--color-secondary)] shrink-0">
            {{ conv.timestamp }}
          </span>
        </div>

        <p
          class="text-xs text-[var(--color-secondary)] line-clamp-2 ml-[52px] leading-relaxed"
        >
          {{ conv.snippet }}
        </p>
      </div>
    </div>
  </aside>
</template>
