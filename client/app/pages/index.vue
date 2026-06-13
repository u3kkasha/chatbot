<script setup lang="ts">
definePageMeta({ layout: "default" });

useSeoMeta({
  title: "Inbox — Zerocium Operator Dashboard",
  description: "Manage and respond to omnichannel customer conversations in real time.",
});

const {
  conversations,
  activeSessionId,
  activeMessages,
  activeProfile,
  isLoading,
  selectSession,
  submitMessage,
} = useInbox();
</script>

<template>
  <div class="relative flex flex-1 h-full overflow-hidden">
    <!-- Loading overlay -->
    <Transition name="fade">
      <div
        v-if="isLoading"
        class="absolute inset-0 z-50 flex items-center justify-center bg-[var(--color-background)]/80 backdrop-blur-sm"
      >
        <div class="flex flex-col items-center gap-3">
          <div
            class="w-8 h-8 border-2 border-[var(--color-primary)] border-t-transparent rounded-full animate-spin"
          />
          <span class="text-sm text-[var(--color-secondary)]">Loading conversations…</span>
        </div>
      </div>
    </Transition>

    <!-- Left pane: conversation list from API -->
    <InboxConversationList
      :conversations="conversations"
      :active-id="activeSessionId ?? ''"
      @select="selectSession"
    />

    <!-- Centre pane: active chat window wired to real messages + send mutation -->
    <InboxChatWindow
      :customer-name="activeProfile?.name ?? '…'"
      :is-priority="activeProfile?.status === 'active'"
      :messages="activeMessages"
      :suggestions="AI_SUGGESTIONS"
      @send="submitMessage"
      @use-suggestion="() => {}"
    />

    <!-- Right pane: customer profile derived from active session -->
    <InboxCustomerProfile
      v-if="activeProfile"
      :profile="activeProfile"
    />
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
