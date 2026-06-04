<script setup lang="ts">
import { useChat } from "@ai-sdk/vue";

const route = useRoute();
const toast = useToast();
const { csrf, headerName } = useCsrf();

const { messages, input, handleSubmit, status, stop, regenerate, error } =
  useChat({
    id: (route.params.id as string) || "default-chat",
    api: "/api/chat",
    headers: {
      [headerName]: csrf,
    },
    onError(error) {
      let message = error.message;
      if (typeof message === "string" && message[0] === "{") {
        try {
          message = JSON.parse(message).message || message;
        } catch {
          // keep original message on malformed JSON
        }
      }

      toast.add({
        description: message,
        icon: "i-lucide-alert-circle",
        color: "error",
        duration: 0,
      });
    },
  });
</script>

<template>
  <UDashboardPanel
    id="chat"
    class="relative min-h-0"
    :ui="{ body: 'p-0 sm:p-0 overscroll-none' }"
  >
    <template #header>
      <Navbar>
        <template #title>
          <span class="text-lg font-semibold">New Chat</span>
        </template>
      </Navbar>
    </template>

    <template #body>
      <div class="flex flex-1">
        <UContainer class="flex flex-1 flex-col gap-4 sm:gap-6">
          <UChatMessages
            should-auto-scroll
            :messages="messages"
            :status="status"
            class="pt-(--ui-header-height) pb-4 sm:pb-6"
          >
            <template #indicator>
              <div class="flex items-center gap-1.5">
                <ChatIndicator />
                <UChatShimmer text="Thinking..." class="text-sm" />
              </div>
            </template>

            <template #content="{ message }">
              <ChatMessageContent :message="message" :editing="false" />
            </template>
          </UChatMessages>

          <UChatPrompt
            v-model="input"
            :error="error"
            variant="subtle"
            class="sticky bottom-0 z-10 rounded-b-none [view-transition-name:chat-prompt]"
            :ui="{ base: 'px-1.5' }"
            @submit="handleSubmit"
          >
            <template #footer>
              <div class="flex items-center gap-1">
                <!-- Actions go here later -->
              </div>

              <UChatPromptSubmit
                :status="status"
                color="neutral"
                size="sm"
                @stop="stop()"
                @reload="regenerate()"
              />
            </template>
          </UChatPrompt>
        </UContainer>
      </div>
    </template>
  </UDashboardPanel>
</template>
