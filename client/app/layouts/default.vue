<script setup lang="ts">
const { toggle, sidebarClass, contentClass, headerClass, toggleIcon, isExpanded } =
  useSidebar();
</script>

<template>
  <div class="flex min-h-screen bg-[var(--color-background)]">
    <!-- ── SideNavBar ──────────────────────────────────────────── -->
    <nav
      :class="sidebarClass"
      class="bg-[var(--color-surface)] border-r border-[var(--color-outline-variant)] h-screen fixed left-0 top-0 flex flex-col py-6 z-40 transition-all duration-300 hidden md:flex"
    >
      <!-- Brand + toggle -->
      <div class="px-4 mb-8 flex justify-between items-center">
        <div class="hide-on-collapse px-2">
          <h1 class="text-2xl font-bold text-[var(--color-primary)] tracking-tight">
            Zerocium
          </h1>
          <p class="text-xs text-[var(--color-secondary)] mt-1">Omnichannel OS</p>
        </div>
        <UButton
          id="toggleSidebar"
          color="neutral"
          variant="ghost"
          :icon="toggleIcon"
          class="shrink-0 mx-auto rounded-full"
          aria-label="Toggle sidebar"
          @click="toggle"
        />
      </div>

      <!-- Nav links -->
      <div class="flex-1 flex flex-col gap-1">
        <NuxtLink
          to="/"
          title="Inbox"
          class="flex items-center gap-3 px-4 py-3 transition-all duration-200 border-l-4"
          active-class="bg-[var(--color-secondary-container)] text-[var(--color-primary)] border-[var(--color-primary)]"
          inactive-class="text-[var(--color-secondary)] hover:bg-[var(--color-surface-container-high)] border-transparent"
        >
          <UIcon name="i-material-symbols-inbox" class="shrink-0 text-xl" />
          <span class="text-xs font-semibold tracking-widest uppercase hide-on-collapse">
            Inbox
          </span>
        </NuxtLink>

        <NuxtLink
          to="/customers"
          title="Customers"
          class="flex items-center gap-3 px-4 py-3 transition-colors duration-200 border-l-4"
          active-class="bg-[var(--color-secondary-container)] text-[var(--color-primary)] border-[var(--color-primary)]"
          inactive-class="text-[var(--color-secondary)] hover:bg-[var(--color-surface-container-high)] border-transparent"
        >
          <UIcon name="i-material-symbols-group-outline" class="shrink-0 text-xl" />
          <span class="text-xs font-semibold tracking-widest uppercase hide-on-collapse">
            Customers
          </span>
        </NuxtLink>

        <NuxtLink
          to="/analytics"
          title="Analytics"
          class="flex items-center gap-3 px-4 py-3 transition-colors duration-200 border-l-4"
          active-class="bg-[var(--color-secondary-container)] text-[var(--color-primary)] border-[var(--color-primary)]"
          inactive-class="text-[var(--color-secondary)] hover:bg-[var(--color-surface-container-high)] border-transparent"
        >
          <UIcon name="i-material-symbols-monitoring" class="shrink-0 text-xl" />
          <span class="text-xs font-semibold tracking-widest uppercase hide-on-collapse">
            Analytics
          </span>
        </NuxtLink>

        <NuxtLink
          to="/settings"
          title="Settings"
          class="flex items-center gap-3 px-4 py-3 transition-colors duration-200 border-l-4"
          active-class="bg-[var(--color-secondary-container)] text-[var(--color-primary)] border-[var(--color-primary)]"
          inactive-class="text-[var(--color-secondary)] hover:bg-[var(--color-surface-container-high)] border-transparent"
        >
          <UIcon name="i-material-symbols-settings-outline" class="shrink-0 text-xl" />
          <span class="text-xs font-semibold tracking-widest uppercase hide-on-collapse">
            Settings
          </span>
        </NuxtLink>
      </div>

      <!-- Operator profile footer -->
      <div class="px-4 mt-auto">
        <div
          class="flex items-center gap-3 px-2 py-3 text-[var(--color-secondary)]"
          :class="isExpanded ? 'justify-start' : 'justify-center'"
        >
          <!-- Letter avatar — compliant with animate-beings restriction -->
          <div
            class="w-8 h-8 rounded-full bg-[var(--color-primary)] text-white flex items-center justify-center text-xs font-bold shrink-0 ambient-shadow"
            aria-label="Operator avatar"
          >
            AM
          </div>
          <div class="hide-on-collapse">
            <p class="text-xs font-semibold text-[var(--color-on-surface)]">Alex Mercer</p>
            <p class="text-[11px] text-[var(--color-secondary)]">Online</p>
          </div>
        </div>
      </div>
    </nav>

    <!-- ── TopAppBar ───────────────────────────────────────────── -->
    <header
      :class="headerClass"
      class="bg-[var(--color-surface-bright)] border-b border-[var(--color-outline-variant)] h-16 fixed top-0 right-0 z-30 flex justify-between items-center px-6 transition-all duration-300"
    >
      <div class="flex items-center gap-4">
        <h2 class="text-lg font-semibold text-[var(--color-on-surface)]">
          Operator Dashboard
        </h2>
      </div>

      <div class="flex items-center gap-3">
        <!-- Search -->
        <div class="relative hidden sm:block">
          <UIcon
            name="i-material-symbols-search"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-[var(--color-secondary)] text-lg"
          />
          <input
            type="text"
            placeholder="Search…"
            class="bg-[var(--color-surface-container-lowest)] border border-[var(--color-outline-variant)] rounded-full pl-10 pr-4 py-1.5 text-xs focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] focus:border-transparent w-60 placeholder:text-[var(--color-secondary)]"
          >
        </div>

        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-notifications-outline"
          class="rounded-full"
          aria-label="Notifications"
        />
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-material-symbols-help-outline"
          class="rounded-full"
          aria-label="Help"
        />

        <!-- Online indicator -->
        <div
          class="flex items-center gap-2 ml-2 pl-4 border-l border-[var(--color-outline-variant)]"
        >
          <span class="w-2.5 h-2.5 rounded-full bg-[var(--color-primary)] animate-pulse" />
          <span class="text-xs font-semibold text-[var(--color-primary)]">Online</span>
        </div>
      </div>
    </header>

    <!-- ── Page slot ───────────────────────────────────────────── -->
    <main
      :class="contentClass"
      class="flex-1 mt-16 flex h-[calc(100vh-4rem)] transition-all duration-300 w-full"
    >
      <slot />
    </main>
  </div>
</template>
