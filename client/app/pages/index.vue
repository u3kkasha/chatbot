<script setup lang="ts">
import type { ConversationItem } from "~/components/inbox/ConversationList.vue";
import type { ChatMessage, AiSuggestion } from "~/components/inbox/ChatWindow.vue";
import type { CustomerProfile } from "~/components/inbox/CustomerProfile.vue";

definePageMeta({ layout: "default" });

useSeoMeta({
  title: "Inbox — Zerocium Operator Dashboard",
  description: "Manage and respond to omnichannel customer conversations in real time.",
});

// ── Mock data ─────────────────────────────────────────────────────────────────

const conversations = ref<ConversationItem[]>([
  {
    id: "conv-1",
    name: "Sarah Jenkins",
    initials: "SJ",
    channel: "live-chat",
    snippet: "I'm having trouble updating my billing information. It keeps giving me an error code 402.",
    timestamp: "2m ago",
    isActive: true,
  },
  {
    id: "conv-2",
    name: "Michael Ross",
    initials: "MR",
    channel: "whatsapp",
    snippet: "Can you confirm if my recent order #88921 has shipped yet?",
    timestamp: "15m ago",
  },
  {
    id: "conv-3",
    name: "David Chen",
    initials: "DC",
    channel: "email",
    snippet: "Thank you for resolving the issue. I appreciate the quick response.",
    timestamp: "1h ago",
  },
]);

const activeConvId = ref("conv-1");

const messages = ref<ChatMessage[]>([
  {
    id: "msg-1",
    role: "customer",
    text: "Hi, I'm trying to update my credit card info for my subscription, but every time I hit save, I get an \"Error Code 402\". Can you help?",
  },
  {
    id: "msg-2",
    role: "agent",
    text: "Hello Sarah! I can certainly help with that. Error 402 usually means there's a temporary block from your bank for online recurring transactions. Let me check your account status real quick.",
  },
  {
    id: "msg-3",
    role: "customer",
    text: "Oh, okay. That's strange, I just used it yesterday. Please let me know what you find.",
  },
]);

const suggestions = ref<AiSuggestion[]>([
  {
    id: "s-1",
    text: "I see the issue. I can send you a secure link to bypass the error and re-enter your card details.",
  },
  {
    id: "s-2",
    text: "Your bank requires a 3D secure verification. Please check your banking app for an approval prompt.",
  },
  {
    id: "s-3",
    text: "Let me reset the payment gateway on our end and you can try again in a few minutes.",
  },
]);

const customerProfile = ref<CustomerProfile>({
  name: "Sarah Jenkins",
  initials: "SJ",
  status: "active",
  email: "sarah.j@example.com",
  phone: "+1 555-0123",
  location: "New York, USA",
  tags: ["Premium", "Beta User", "Billing Issue"],
  about:
    "Customer since Jan 2022. Currently on the Pro Tier Plan. Frequent user of the mobile app and dashboard analytics.",
});

// ── Handlers ──────────────────────────────────────────────────────────────────

function selectConversation(id: string) {
  activeConvId.value = id;
}

function sendMessage(text: string) {
  messages.value.push({
    id: `msg-${Date.now()}`,
    role: "agent",
    text,
  });
}
</script>

<template>
  <div class="flex flex-1 h-full overflow-hidden">
    <!-- Left pane: conversation list -->
    <InboxConversationList
      :conversations="conversations"
      :active-id="activeConvId"
      @select="selectConversation"
    />

    <!-- Centre pane: active chat window -->
    <InboxChatWindow
      :customer-name="customerProfile.name"
      :is-priority="true"
      :messages="messages"
      :suggestions="suggestions"
      @send="sendMessage"
      @use-suggestion="(t) => (/* draft handled inside component */ t)"
    />

    <!-- Right pane: customer profile -->
    <InboxCustomerProfile :profile="customerProfile" />
  </div>
</template>
