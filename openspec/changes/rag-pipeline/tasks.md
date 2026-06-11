# Tasks: RAG Pipeline

## Phase 1: Configuration & Brokers

- [x] Create `AIOptions`, `QdrantOptions`, and `ProcessingOptions` records with `[OptionsValidator]`.
- [x] Configure DI bindings for options in `SharedExtensions` avoiding eager validation.
- [x] Implement `AIBroker` in `Shared/Brokers/AI` using `Microsoft.Extensions.AI` and `RESTFulSense` for reranking.
- [x] Implement `QdrantBroker` in `Shared/Brokers/Vector` using `Qdrant.Client` and `QueryAsync` with `Fusion.Rrf`.
- [x] Ensure `ProcessingBroker` utilizes Docling's native chunking endpoints.

## Phase 2: Knowledge Services

- [x] Implement a custom `SparseVectorService` for BM25 sparse vector generation (pure C#, no ONNX).
- [x] Create `KnowledgeChunk` DTO.
- [x] Implement `KnowledgeFoundationService` orchestrating the two-stage retrieval flow: Dense + Sparse Embed -> Qdrant Hybrid Search (Top 50) -> OpenRouter Rerank (Top 5).
- [x] Wrap native exceptions into `DependencyException` and return `OneOf`.

## Phase 3: Agent Integration

- [x] Update `ChatFoundationService` to initialize `ChatClientAgent`.
- [x] Configure `TextSearchProvider` to execute `BeforeAIInvoke`.
- [x] Wire the provider's search delegate to `KnowledgeFoundationService`.

## Phase 4: Testing & Verification

- [x] Write unit tests for `SparseVectorService` (custom BM25 logic).
- [x] Write unit tests for `KnowledgeFoundationService` ensuring exception wrapping into `OneOf`.
- [ ] Write integration tests for the Qdrant connection and Agent flow.
