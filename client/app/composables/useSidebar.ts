/**
 * Composable that controls the collapsible sidebar state.
 * Shared across the layout and any component that needs to react to sidebar width.
 */
export const useSidebar = () => {
  const isExpanded = useState("sidebar-expanded", () => true);

  const toggle = () => {
    isExpanded.value = !isExpanded.value;
  };

  const sidebarClass = computed(() =>
    isExpanded.value ? "sidebar-expanded" : "sidebar-collapsed",
  );

  const contentClass = computed(() =>
    isExpanded.value ? "content-expanded" : "content-collapsed",
  );

  const headerClass = computed(() =>
    isExpanded.value ? "header-expanded" : "header-collapsed",
  );

  const toggleIcon = computed(() =>
    isExpanded.value ? "i-material-symbols-menu-open" : "i-material-symbols-menu",
  );

  return { isExpanded, toggle, sidebarClass, contentClass, headerClass, toggleIcon };
};
