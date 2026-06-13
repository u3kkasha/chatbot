<script setup lang="ts">
export interface CustomerProfile {
  name: string;
  initials: string;
  status: "active" | "away" | "offline";
  email: string;
  phone: string;
  location: string;
  tags: string[];
  about: string;
}

defineProps<{
  profile: CustomerProfile;
}>();

const statusColors: Record<CustomerProfile["status"], string> = {
  active: "#0d7a46",
  away: "#c87c00",
  offline: "#737686",
};

const statusLabels: Record<CustomerProfile["status"], string> = {
  active: "Active Now",
  away: "Away",
  offline: "Offline",
};

const tagVariants = [
  "bg-[var(--color-primary-container)] text-[var(--color-on-primary-container)]",
  "bg-[var(--color-secondary-container)] text-[var(--color-on-secondary-container)]",
  "bg-[var(--color-error-container)] text-[var(--color-on-error-container)] border border-[#ffb4ab]",
  "bg-[var(--color-surface-container-high)] text-[var(--color-secondary)]",
];
</script>

<template>
  <aside
    class="w-[300px] bg-[var(--color-surface-bright)] border-l border-[var(--color-outline-variant)] flex flex-col hidden xl:flex overflow-y-auto shrink-0"
  >
    <!-- Customer header -->
    <div
      class="p-6 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] flex flex-col items-center text-center"
    >
      <!-- Letter avatar with online indicator -->
      <div class="relative mb-3">
        <div
          class="w-20 h-20 rounded-full bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-surface-tint)] text-white flex items-center justify-center text-2xl font-bold ambient-shadow"
          aria-label="Customer avatar"
        >
          {{ profile.initials }}
        </div>
        <span
          class="absolute bottom-1 right-1 w-4 h-4 border-2 border-white rounded-full"
          :style="{ backgroundColor: statusColors[profile.status] }"
        />
      </div>
      <h3 class="text-lg font-semibold text-[var(--color-on-surface)]">
        {{ profile.name }}
      </h3>
      <p
        class="text-[11px] font-semibold mt-1"
        :style="{ color: statusColors[profile.status] }"
      >
        {{ statusLabels[profile.status] }}
      </p>
    </div>

    <!-- Details -->
    <div class="p-6 space-y-6">
      <!-- Contact info -->
      <div>
        <h4
          class="text-[11px] font-semibold text-[var(--color-secondary)] uppercase tracking-wider mb-3"
        >
          Contact Info
        </h4>
        <div class="space-y-3 text-xs">
          <div class="flex items-center gap-3">
            <UIcon
              name="i-material-symbols-mail-outline"
              class="text-[var(--color-tertiary)] text-lg shrink-0"
            />
            <span class="text-[var(--color-on-surface)]">{{ profile.email }}</span>
          </div>
          <div class="flex items-center gap-3">
            <UIcon
              name="i-material-symbols-call-outline"
              class="text-[var(--color-tertiary)] text-lg shrink-0"
            />
            <span class="text-[var(--color-on-surface)]">{{ profile.phone }}</span>
          </div>
          <div class="flex items-center gap-3">
            <UIcon
              name="i-material-symbols-location-on-outline"
              class="text-[var(--color-tertiary)] text-lg shrink-0"
            />
            <span class="text-[var(--color-on-surface)]">{{ profile.location }}</span>
          </div>
        </div>
      </div>

      <hr class="border-[var(--color-outline-variant)]">

      <!-- Tags -->
      <div>
        <h4
          class="text-[11px] font-semibold text-[var(--color-secondary)] uppercase tracking-wider mb-3"
        >
          Tags
        </h4>
        <div class="flex flex-wrap gap-2">
          <span
            v-for="(tag, i) in profile.tags"
            :key="tag"
            class="px-2 py-1 rounded text-[11px] font-semibold"
            :class="tagVariants[i % tagVariants.length]"
          >
            {{ tag }}
          </span>
        </div>
      </div>

      <hr class="border-[var(--color-outline-variant)]">

      <!-- About -->
      <div>
        <h4
          class="text-[11px] font-semibold text-[var(--color-secondary)] uppercase tracking-wider mb-3"
        >
          About
        </h4>
        <p class="text-xs text-[var(--color-on-surface-variant)] leading-relaxed">
          {{ profile.about }}
        </p>
      </div>
    </div>
  </aside>
</template>
