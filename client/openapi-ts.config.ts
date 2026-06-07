import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "./openapi.json",
  output: "./app/api-client",
  plugins: [
    "@hey-api/client-ofetch",
    "valibot",
    {
      name: "@hey-api/transformers",
      dates: true, // Maps .NET System.DateTime natively to Date
      bigInt: true, // Maps .NET long / Int64 values natively to BigInt
    },
    {
      name: "@hey-api/sdk",
      validator: true,
      transformer: true,
    },
    {
      name: "@pinia/colada",
      queryOptions: true,
      mutationOptions: true,
      queryKeys: {
        tags: true,
      },
    },
  ],
});
