# Tasks: RAG Pipeline

## Phase 1: Configuration & Brokers

- [ ] Create `AIOptions`, `QdrantOptions`, and `ProcessingOptions` records with `[OptionsValidator]`.
- [ ] Configure DI bindings for options in `SharedExtensions` avoiding eager validation.
- [ ] Implement `AIBroker` in `Shared/Brokers/AI` using `Microsoft.Extensions.AI` and `RESTFulSense` for reranking.
- [ ] Implement `QdrantBroker` in `Shared/Brokers/Vector` using `Qdrant.Client` and `QueryAsync` with `Fusion.Rrf`.
- [ ] Ensure `ProcessingBroker` utilizes Docling's native chunking endpoints.

## Phase 2: Knowledge Services

- [ ] Implement a custom `SparseVectorService` for BM25 sparse vector generation (pure C#, no ONNX).
- [ ] Create `KnowledgeChunk` DTO.
- [ ] Implement `KnowledgeFoundationService` orchestrating the two-stage retrieval flow: Dense + Sparse Embed -> Qdrant Hybrid Search (Top 50) -> OpenRouter Rerank (Top 5).
- [ ] Wrap native exceptions into `DependencyException` and return `OneOf`.

## Phase 3: Agent Integration

- [ ] Update `ChatFoundationService` to initialize `ChatClientAgent`.
- [ ] Configure `TextSearchProvider` to execute `BeforeAIInvoke`.
- [ ] Wire the provider's search delegate to `KnowledgeFoundationService`.

## Phase 4: Testing & Verification

- [ ] Write unit tests for `SparseVectorService` (custom BM25 logic).
- [ ] Write unit tests for `KnowledgeFoundationService` ensuring exception wrapping into `OneOf`.
- [ ] Write integration tests for the Qdrant connection and Agent flow.
