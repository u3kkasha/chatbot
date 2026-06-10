# RAG Pipeline Implementation

## Summary

Implement a Retrieval-Augmented Generation (RAG) pipeline for the Chatbot using Microsoft Agent Framework, Qdrant for vector search, Docling for document chunking, and OpenRouter for LLM, Dense Embeddings (Qwen), and Cross-Encoder Reranking (Cohere). Sparse embeddings for hybrid search will be generated natively in C# using a custom BM25 implementation.

## Motivation

To provide grounded, context-aware responses to users by querying a local knowledge base. Fast and highly accurate responses are prioritized through a two-stage retrieval process: Qdrant for initial hybrid retrieval, and Cohere for precise ML reranking. Document processing relies on Docling's native capabilities to ensure high-quality, semantic chunking without re-implementing logic in C#.

## Scope

- Setup strongly-typed configuration for AI, Qdrant, and Processing (Docling) environments.
- Implement OpenRouter-based `IAIBroker` for:
  1. Dense embeddings using `Microsoft.Extensions.AI` (Qwen).
  2. Chat generation.
  3. **Cross-Encoder Reranking** via OpenRouter's `/api/v1/rerank` endpoint (using `cohere/rerank-v3.5` via `RESTFulSense`).
- Implement a custom, high-performance C# **BM25 Sparse Vector Generator** (no ONNX required).
- Ensure document ingestion utilizes Docling's native chunking capabilities via the `ProcessingBroker`.
- Implement `IQdrantBroker` for hybrid vector search (fetching top 50 results using `QueryAsync` with `Fusion.Rrf`).
- Create `IKnowledgeFoundationService` to orchestrate: Embed (Dense from AI + Sparse from Custom C#) -> Qdrant Hybrid Search (Top 50) -> OpenRouter Rerank (Top 5).
- Wire up `ChatClientAgent` with `TextSearchProvider` in `Modules.Chat` to inject retrieved context prior to AI invocation.
