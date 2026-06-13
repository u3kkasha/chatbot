import { useQuery, useMutation, useQueryCache } from "@pinia/colada";
import {
  getAllChatSessionsQuery,
  getAllChatMessagesQuery,
  createChatMessageMutation,
  getAllChatMessagesQueryKey,
} from "~/api-client/@pinia/colada.gen";
import type { ChatSessionResponse, ChatMessageResponse } from "~/api-client";
import type { ConversationItem } from "~/components/inbox/ConversationList.vue";
import type { ChatMessage, AiSuggestion } from "~/components/inbox/ChatWindow.vue";
import type { CustomerProfile } from "~/components/inbox/CustomerProfile.vue";

// ── Channel mapping ────────────────────────────────────────────────────────────
const CHANNEL_MAP: Record<string, ConversationItem["channel"]> = {
  WebWidget: "live-chat",
  WhatsApp: "whatsapp",
  Email: "email",
  TikTok: "live-chat",
  Facebook: "live-chat",
};

// ── Helpers ───────────────────────────────────────────────────────────────────

function toInitials(name: string): string {
  return name
    .split(" ")
    .map((w) => w[0] ?? "")
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

/**
 * Format a NodaTime Instant (serialised as ISO-8601 string or opaque object)
 * into a human-readable relative timestamp.
 */
function toRelativeTime(instant: unknown): string {
  if (!instant) return "";
  // Transformers may have converted it to a Date already
  const date
    = instant instanceof Date
      ? instant
      : typeof instant === "string"
        ? new Date(instant as string)
        : null;

  if (!date || Number.isNaN(date.getTime())) return "";

  const diff = Date.now() - date.getTime();
  if (diff < 60_000) return "just now";
  if (diff < 3_600_000) return `${Math.floor(diff / 60_000)}m ago`;
  if (diff < 86_400_000) return `${Math.floor(diff / 3_600_000)}h ago`;
  return `${Math.floor(diff / 86_400_000)}d ago`;
}

function mapSessionToItem(
  session: ChatSessionResponse,
  snippet: string,
): ConversationItem {
  return {
    id: session.id,
    name: session.customer_identifier,
    initials: toInitials(session.customer_identifier),
    channel: CHANNEL_MAP[session.channel_provider] ?? "live-chat",
    snippet,
    timestamp: toRelativeTime(session.updated_date),
  };
}

function mapMessageToChat(m: ChatMessageResponse): ChatMessage {
  return {
    id: m.id,
    role: m.sender === "Customer" ? "customer" : "agent",
    text: m.content,
  };
}

function buildProfile(session: ChatSessionResponse): CustomerProfile {
  const name = session.customer_identifier;
  const status = session.status === "Open" ? "active" : session.status === "Pending" ? "away" : "offline";

  // Match seeded demo profiles for a highly realistic, premium experience
  if (name === "Sarah Jenkins") {
    return {
      name,
      initials: "SJ",
      status,
      email: "sarah.j@example.com",
      phone: "+1 555-0123",
      location: "New York, USA",
      tags: ["Premium", "Beta User", "Billing Issue"],
      about: "Customer since Jan 2022. Currently on the Pro Tier Plan. Frequent user of the mobile app and dashboard analytics.",
    };
  }

  if (name === "Michael Ross") {
    return {
      name,
      initials: "MR",
      status,
      email: "m.ross@example.com",
      phone: session.external_reference_id ?? "+1 555-0198",
      location: "London, UK",
      tags: ["WhatsApp", "Standard"],
      about: "Customer since March 2023. Prefers communicating via WhatsApp. Interested in enterprise upgrade.",
    };
  }

  if (name === "David Chen") {
    return {
      name,
      initials: "DC",
      status,
      email: session.external_reference_id ?? "david.chen@example.com",
      phone: "+1 555-0742",
      location: "San Francisco, USA",
      tags: ["Email", "Resolved"],
      about: "Customer since June 2021. Legacy account holder. Integrates with Slack and Salesforce.",
    };
  }

  // Fallback for new dynamically created sessions
  const isEmail = session.external_reference_id?.includes("@");
  return {
    name,
    initials: toInitials(name),
    status,
    email: isEmail
      ? (session.external_reference_id ?? `${name.toLowerCase().replace(/\s+/g, ".")}@example.com`)
      : `${name.toLowerCase().replace(/\s+/g, ".")}@example.com`,
    phone: session.channel_provider === "WhatsApp"
      ? (session.external_reference_id ?? "")
      : "",
    location: "Unknown Location",
    tags: [session.channel_provider, session.status],
    about: `Customer on ${session.channel_provider} channel. Session status: ${session.status}.`,
  };
}

// ── Composable ────────────────────────────────────────────────────────────────

/** Static AI reply suggestions — will be replaced by SSE stream later. */
export const AI_SUGGESTIONS: AiSuggestion[] = [
  {
    id: "s-1",
    text: "I see the issue. I can send you a secure link to bypass the error and re-enter your card details.",
  },
  {
    id: "s-2",
    text: "Your bank requires 3D Secure verification. Please check your banking app for an approval prompt.",
  },
  {
    id: "s-3",
    text: "Let me reset the payment gateway on our end. Please try again in a few minutes.",
  },
];

export function useInbox() {
  const queryCache = useQueryCache();

  // Persisted across navigation via useState (SSR-safe)
  const activeSessionId = useState<string | null>("inbox-active-session", () => null);

  // ── Queries ────────────────────────────────────────────────────────────────
  const { data: sessions, status: sessionsStatus } = useQuery(
    getAllChatSessionsQuery(),
  );

  const { data: allMessages, status: messagesStatus } = useQuery(
    getAllChatMessagesQuery(),
  );

  // ── Derived state ──────────────────────────────────────────────────────────

  /** Auto-select the first session once data loads */
  watchEffect(() => {
    if (!activeSessionId.value && (sessions.value?.length ?? 0) > 0) {
      activeSessionId.value = sessions.value![0]!.id;
    }
  });

  const conversations = computed<ConversationItem[]>(() => {
    const msgs = allMessages.value ?? [];
    return (sessions.value ?? []).map((s) => {
      const sessionMsgs = msgs.filter((m) => m.session_id === s.id);
      const latest = sessionMsgs[sessionMsgs.length - 1];
      return mapSessionToItem(s, latest?.content ?? "No messages yet");
    });
  });

  const activeSession = computed(
    () => (sessions.value ?? []).find((s) => s.id === activeSessionId.value) ?? null,
  );

  const activeMessages = computed<ChatMessage[]>(() =>
    (allMessages.value ?? [])
      .filter((m) => m.session_id === activeSessionId.value)
      .map(mapMessageToChat),
  );

  const activeProfile = computed<CustomerProfile | null>(() =>
    activeSession.value ? buildProfile(activeSession.value) : null,
  );

  const isLoading = computed(
    () => sessionsStatus.value === "pending" || messagesStatus.value === "pending",
  );

  // ── Mutation ───────────────────────────────────────────────────────────────
  const { mutate: sendMessage, status: sendStatus } = useMutation({
    ...createChatMessageMutation(),
    async onSuccess() {
      // Refresh message list so the sent message appears immediately
      await queryCache.invalidateQueries({ key: getAllChatMessagesQueryKey() });
    },
  });

  function submitMessage(text: string) {
    if (!activeSession.value || !text.trim()) return;
    sendMessage({
      body: {
        session_id: activeSession.value.id,
        tenant_id: activeSession.value.tenant_id,
        sender: "Operator",
        content: text.trim(),
        status: "Sent",
        is_ai_generated: false,
        approved_by: null,
        citations: null,
        ai_metadata: null,
      },
    });
  }

  return {
    // State
    conversations,
    activeSessionId,
    activeMessages,
    activeProfile,
    isLoading,
    sendStatus,
    // Actions
    selectSession: (id: string) => {
      activeSessionId.value = id;
    },
    submitMessage,
  };
}
