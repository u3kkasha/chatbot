# Verification: RAG AI Quality Proof

This document defines the strategy for proving the **AI Quality** of the RAG pipeline. It focuses on retrieval accuracy and groundedness without relying on the Chat UI or PostgreSQL.

## 1. Objective

To provide an absolute, empirical proof that the RAG pipeline can ingest, index, and retrieve specific knowledge facts correctly.

## 2. Testing Strategy: The "Synthetic Fact" Probe

We use a "Needle in a Haystack" approach where a unique, fictional fact is injected into a sea of noise. If the system can retrieve this fact as the top result for a semantic query, the pipeline is proven.

### Fact Definition (The Needle)
- **Format**: Markdown
- **Content**: A fictional protocol or secret code that does not exist in the LLM's training data.
- **Example**:
  ```markdown
  # Protocol Midnight-Alpha
  **Required Action**: Execute a `VORTEX-GHOST-7` sequence.
  **Authorized By**: General Xylos.
  ```

### Noise Definition (The Haystack)
- **Format**: 50-100 random Markdown chunks about unrelated topics (e.g., microwave manuals, gardening tips).

## 3. Hermetic Test Environment

The test must be fully self-contained (Hermetic) except for the live AI provider.

### Infrastructure (Testcontainers)
- **Qdrant**: Ephemeral vector database.
- **Docling-Serve**: Ephemeral document processing service (`quay.io/docling-project/docling-serve:latest`).
- **Postgres**: **NOT REQUIRED**. We bypass the relational database to focus on AI quality.

### Live AI Provider (OpenRouter)
- **Secret**: `AI__ApiKey` must be sourced from the environment (Infisical).
- **Condition**: Test is skipped if the API Key is missing (using `LiveFact`).

## 4. Execution Flow (The "Quality Proof" Test)

1. **Arrange**:
   - Initialize Testcontainers (Qdrant + Docling).
   - Generate a `tenant_id` (Guid).
   - Prepare the "Needle" Markdown and "Haystack" noise.
2. **Act (Ingest)**:
   - Call `ProcessingBroker` to chunk the Markdown.
   - Call `AiBroker` to generate Dense Embeddings.
   - Call `SparseVectorService` to generate BM25 Vectors.
   - Call `VectorBroker` to Upsert all points into Qdrant.
3. **Act (Retrieve)**:
   - Call `KnowledgeFoundationService.RetrieveAsync("What is the action for Midnight-Alpha?")`.
4. **Assert (Verify)**:
   - **Presence**: `results.ShouldNotBeEmpty()`.
   - **Rank**: `results[0].Content.ShouldContain("VORTEX-GHOST-7")`.
   - **Quality**: `results[0].Score` should be significantly higher than `results[1].Score`.
   - **Citations**: `results[0].DocumentId` must match the "Needle" ID.

## 5. Success Criteria

A RAG implementation is considered "Proven" if it passes this test across three specific query types:
1. **Keyword Match**: "Midnight-Alpha" (Proves Sparse/BM25).
2. **Semantic Match**: "What should I do during the 12AM protocol?" (Proves Dense/Embedding).
3. **Complex Layout**: Fact hidden in a table or list (Proves Docling Chunking).
