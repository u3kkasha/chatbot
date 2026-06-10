# Design: RAG Pipeline

## Core Architecture

The backend follows a Modular Monolith architecture integrating Vertical Slice Architecture (VSA) and The Standard. Business logic returns `OneOf` unions, and exceptions are restricted to technical failures mapped as `DependencyException` or `ServiceException`.

### 1. Configuration (`appsettings.json` / ENV mapping)

**Constraint:** Follow "The Standard" configuration guidelines for AOT compatibility.

- Bind `AIOptions` (`Endpoint`, `ApiKey`, `ModelId`, `EmbeddingModelId`, `RerankModelId`), `QdrantOptions`, and `ProcessingOptions`.
- Use `AddOptions<T>()` and `[OptionsValidator]`. Avoid eager `GetSection()[...]` calls during DI registration. Validate at startup conditionally only when not generating OpenAPI docs.

### 2. Brokers (`Shared/Brokers`)

Brokers wrap external SDKs and APIs. They do not hold business logic or return `OneOf`. They throw native exceptions.

- **`IAIBroker`**: Wraps `Microsoft.Extensions.AI.IChatClient` and `IEmbeddingGenerator`. Configured to point to OpenRouter. Injects `IRESTFulApiFactoryClient` (RESTFulSense) to call OpenRouter's `/api/v1/rerank` endpoint.
- **`IQdrantBroker`**: Wraps `Qdrant.Client.Grpc.QdrantClient`. Exposes `SearchAsync` leveraging `QueryAsync` with `Fusion.Rrf` using both dense and sparse prefetch queries.
- **`IProcessingBroker`**: Interacts with the Docling-serve microservice. Crucially, delegates **all document chunking** to Docling natively; no chunking logic exists in C#.

### 3. Services (`Modules.Knowledge`)

- **`ISparseVectorService`**: A custom, lightweight C# implementation of BM25.
  - Takes a string query, tokenizes it (e.g., using `Microsoft.ML.Tokenizers` or a custom regex/whitespace splitter), calculates term frequencies, and returns the sparse vector `(uint[] indices, float[] values)`. No ONNX dependencies are used.
- **`IKnowledgeFoundationService`**: The core domain logic for retrieving knowledge chunks.
  - **Inputs**: User query string.
  - **Process**:
    1. Calls `IAIBroker.GenerateEmbeddingAsync` to get the dense vector (Qwen).
    2. Calls `ISparseVectorService.GenerateAsync` to get the custom BM25 sparse vector.
    3. Calls `IQdrantBroker.SearchAsync` passing both vectors to get a broad candidate pool (Top 50 via RRF).
    4. Calls `IAIBroker.RerankAsync` passing the query and the 50 candidate documents to OpenRouter using `cohere/rerank-v3.5`.
  - **Outputs**: Maps the reranked top 5 results to `KnowledgeChunk` DTOs.
  - **Returns**: `ValueTask<OneOf<IReadOnlyList<KnowledgeChunk>, ServiceException, DependencyException>>`.

### 4. Agent Integration (`Modules.Chat`)

- **`IChatFoundationService`**:
  - Resolves `ChatClientAgent`.
  - Attaches `TextSearchProvider` configured to run `SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke`.
  - The provider's search function delegates to `IKnowledgeFoundationService`.

## Visual Flow

```text
┌────────────────────────────────────────────────────────┐
│ Modules.Chat: ChatFoundationService                    │
│                                                        │
│ 1. Start ChatClientAgent                               │
│ 2. TextSearchProvider triggers BEFORE invoke           │
└──────┬─────────────────────────────────────────────────┘
       │ delegates query
       ▼
┌────────────────────────────────────────────────────────┐
│ Modules.Knowledge: KnowledgeFoundationService          │
│                                                        │
│ 1. AIBroker -> Get Dense Vector (Qwen)                 │
│ 2. SparseVectorService -> Custom BM25 Sparse Vector    │
│ 3. QdrantBroker -> Hybrid Search (Top 50 via RRF)      │
│ 4. AIBroker -> Rerank (Top 50 -> Top 5 via Cohere)     │
│ 5. Return OneOf<IReadOnlyList<KnowledgeChunk>, ...>    │
└──────┬───────────────────────────┬─────────────────────┘
       │                           │
       ▼                           ▼
[ OpenRouter API ]            [ Qdrant DB ]
```
