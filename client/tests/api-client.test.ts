import { describe, it, expect, vi } from "vitest";
import * as v from "valibot";
import {
  vProblemDetails,
  vStreamCompletionQuery,
} from "../app/api-client/valibot.gen";
import {
  getIdentityQueryKey,
  streamCompletionQueryKey,
  getIdentityQuery,
} from "../app/api-client/@pinia/colada.gen";
import { getIdentity } from "../app/api-client/sdk.gen";

describe("API Client Codegen Tests", () => {
  describe("Valibot Schema Validation", () => {
    it("vProblemDetails should validate correct ProblemDetails", () => {
      const validProblem = {
        type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        title: "One or more validation errors occurred.",
        status: 400,
        detail: "Validation failed",
        instance: "/api/chat/completions/stream",
      };

      const result = v.safeParse(vProblemDetails, validProblem);
      expect(result.success).toBe(true);
    });

    it("vStreamCompletionQuery should validate correct query params", () => {
      const validQuery = { prompt: "Hello world" };
      const invalidQuery = {};

      expect(v.safeParse(vStreamCompletionQuery, validQuery).success).toBe(
        true,
      );
      expect(v.safeParse(vStreamCompletionQuery, invalidQuery).success).toBe(
        false,
      );
    });
  });

  describe("Pinia Colada Query Keys", () => {
    it("getIdentityQueryKey should include the Chatbot.Api tag", () => {
      const key = getIdentityQueryKey();
      expect(key[0].tags).toContain("Chatbot.Api");
    });

    it("streamCompletionQueryKey should include the Chat tag", () => {
      const key = streamCompletionQueryKey({ query: { prompt: "test" } });
      expect(key[0].tags).toContain("Chat");
    });
  });

  describe("API Requests Integration", () => {
    it("getIdentity should make a fetch call and return data", async () => {
      const mockFetch = vi.fn().mockResolvedValue(
        new Response(JSON.stringify("IdentityModule"), {
          status: 200,
          headers: { "content-type": "application/json" },
        }),
      );
      globalThis.fetch = mockFetch;

      const { data, error } = await getIdentity();

      expect(data).toBe("IdentityModule");
      expect(error).toBeUndefined();
      expect(mockFetch).toHaveBeenCalled();
      const calledUrl = mockFetch.mock.calls[0][0] as string;
      expect(calledUrl).toContain("/identity");
    });

    it("getIdentityQuery should execute the SDK query function", async () => {
      const mockFetch = vi.fn().mockResolvedValue(
        new Response(JSON.stringify("IdentityModule"), {
          status: 200,
          headers: { "content-type": "application/json" },
        }),
      );
      globalThis.fetch = mockFetch;

      const queryOptions = getIdentityQuery();
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const data = await queryOptions.query({} as any);

      expect(data).toBe("IdentityModule");
    });
  });
});
