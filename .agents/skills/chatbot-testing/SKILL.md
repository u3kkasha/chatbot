---
name: Chatbot Testing
description: Enforces TDD discipline, NSubstitute mock setups, Verify snapshot testing, and Testcontainers integration testing.
---

# Chatbot Testing Guidelines

This skill governs the testing strategy for the Chatbot codebase. We use a combination of Unit Testing (driven by NSubstitute and TDD) and Hermetic Integration Testing (driven by Testcontainers and Verify snapshot testing).

## ⚙️ Test-Driven Development (TDD) Discipline

All business logic in services must be developed following a strict TDD loop:

1. **Red:** Write a failing test first.
2. **Verify Failure:** Run the test to confirm it fails specifically for the reason you expect.
3. **Green:** Write the minimum production code required to make the test pass.
4. **Refactor:** Clean up the implementation code (and the test code) while ensuring all tests stay green.

---

## 🔧 Unit Testing Guidelines

Unit tests isolate a single class under test (e.g., a Service) by mocking all external dependencies.

### 1. Key Libraries & Tools

- **Test Framework:** xUnit (`[Fact]` and `[Theory]`).
- **Mocking Library:** **NSubstitute** (not Moq).
  - Example: `var storageBroker = Substitute.For<IStorageBroker>();`
  - Setup returns: `storageBroker.InsertSessionAsync(session).Returns(session);`
  - Verify calls: `await storageBroker.Received(1).InsertSessionAsync(Arg.Any<ChatSession>());`
- **Assertion Library:** Shouldly.
  - Example: `result.ShouldBeOfType<ValidationError>();`

### 2. Test Structure: GWT (Given/When/Then)

Every unit test must be structured using the GWT pattern:

```csharp
[Fact]
public async Task ShouldAddSessionAsync()
{
    // given
    var inputSession = CreateRandomSession();
    var expectedSession = inputSession.DeepClone();
    _storageBrokerMock.InsertSessionAsync(inputSession).Returns(expectedSession);

    // when
    var result = await _sessionService.AddSessionAsync(inputSession);

    // then
    var actualSession = result.AsT0;
    actualSession.ShouldBeEquivalentTo(expectedSession);
    await _storageBrokerMock.Received(1).InsertSessionAsync(inputSession);
}
```

### 3. Testing Functional Results (OneOf)

- Because services return `OneOf<TSuccess, TError1, ...>` instead of throwing validation/domain exceptions, tests must assert on the returned type and its contents.
- Verify that a validation failure returns a `ValidationError` containing the correct field error messages.

### 4. Testing Categorized Exceptions

- For technical dependency errors, services still catch native exceptions and throw mapped exceptions.
- Tests should mock the dependency to throw a native exception (e.g., `SqlException` or `HttpRequestException`), and assert that the service method throws the mapped exception (e.g., `DependencyException`).

---

## ⚡ Integration Testing Guidelines

Integration tests run against real infrastructure using containerized instances to guarantee correctness and isolation.

### 1. Hermetic Isolation (Testcontainers)

- No test should rely on a pre-existing local or shared database.
- Use **Testcontainers** to spin up fresh, ephemeral instances of:
  - **PostgreSQL** for relational storage.
  - **Qdrant** for vector search.
- Ensure database schemas are migrated programmatically during test suite initialization.

### 2. Snapshot Testing (Verify)

- For testing complex outputs (such as JSON API payloads, file chunking results, AI completions, or deep database objects), use **Verify** for snapshot assertions instead of writing verbose assert statements.
- Example:

```csharp
[Fact]
public async Task ShouldRetrieveCompleteChatHistory()
{
    // Arrange & Act
    var result = await _client.GetAsync("/sessions/123/history");
    var content = await result.Content.ReadAsStringAsync();

    // Assert using Verify
    await Verify(content);
}
```

- Snapshot files must be committed to git alongside the test. When updates are expected, run the verify command to accept the new snapshot.
