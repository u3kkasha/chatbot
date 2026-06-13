import { client } from "~/api-client/client.gen";

export default defineNuxtPlugin(() => {
  const config = useSafeRuntimeConfig();

  client.setConfig({
    baseUrl: config.public.apiBase,
  });
});
