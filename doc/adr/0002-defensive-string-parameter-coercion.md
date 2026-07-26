# 2. Defensive String Parameter Coercion for MCP Tools

* **Status**: Accepted
* **Date**: 2026-07-25
* **Authors**: DevStack Architecture Team

## Context and Problem Statement

MCP clients (AI agents) occasionally send array values for string parameters when invoking tools, particularly for fields like `description`, `result`, and `errors`. This causes `System.Text.Json.JsonException` errors during parameter deserialization:

```
System.Text.Json.JsonException: The JSON value could not be converted to System.String. 
Path: $ | LineNumber: 0 | BytePositionInLine: 1.
---> System.InvalidOperationException: Cannot get the value of a token type 'StartArray' as a string.
```

This occurs in `Microsoft.Extensions.AI.AIFunctionFactory.ReflectionAIFunctionDescriptor.GetParameterMarshaller` when the MCP SDK attempts to deserialize tool arguments into method parameters.

The issue affects all string parameters across MCP tools:
- `create_task`: description
- `update_task`: description, result, errors, agent
- `create_deliverable`: description, design, acceptanceCriteria, executionPlan, securityImpact, performanceImpact, testPlan, deploymentPlan
- `update_deliverable`: description, design, acceptanceCriteria, executionPlan, securityImpact, performanceImpact, testPlan, deploymentPlan, agentFeedback, blocking

This is a client-side issue (the LLM formats parameters incorrectly), but the server should be defensive and handle malformed input gracefully rather than crashing.

## Decision Drivers

* **Resilience**: System should handle malformed client input without crashing
* **Developer Experience**: Clear error messages or automatic correction improves usability
* **Maintenance Overhead**: Solution should be centralized and not require changes to every tool method
* **Type Safety**: Solution should not compromise type safety for valid inputs
* **Performance**: Minimal overhead for normal operations

## Considered Options

* **Option A: Custom JsonConverter<string>**: Register a converter that coerces arrays to strings by joining elements with newlines. Applied globally via JsonSerializerOptions or per-parameter via [JsonConverter] attribute.
* **Option B: Request Filter Normalization**: Add a McpRequestFilter that intercepts CallToolRequestParams and normalizes array arguments to strings before they reach the tool method.
* **Option C: Change Parameter Types to JsonElement**: Change string parameters to JsonElement and manually handle deserialization with fallback logic.
* **Option D: Improve Error Handling**: Catch JsonException in McpExceptionHandlingMiddleware and return user-friendly errors to guide the client.

## Decision Outcome

Chosen option: **"Option A: Custom JsonConverter<string>"**, because it provides a centralized, transparent solution that makes all string parameters resilient to array inputs without changing tool signatures or requiring manual deserialization logic in each method.

### Positive Consequences

* **Resilient to Client Errors**: Automatically handles malformed array inputs by converting them to newline-separated strings
* **Zero Tool Code Changes**: No changes required to existing tool method signatures or implementations
* **Centralized Logic**: Single converter class handles the coercion logic for all string parameters
* **Type Safety Preserved**: Valid string inputs are processed normally; only malformed inputs are coerced
* **Minimal Performance Impact**: Converter only activates for array tokens, which are error cases

### Negative Consequences / Risks

* **Global Side Effects**: The converter applies to all string deserialization in the application, potentially masking real type errors
* **Semantic Loss**: Converting `["item1", "item2"]` to `"item1\nitem2"` may not preserve the client's intended meaning
* **Debugging Difficulty**: Silent coercion might make it harder to identify client bugs

**Mitigation**: The converter logs warnings when coercion occurs, making it visible in logs without breaking the request.

## Pros and Cons of the Options

### Option A: Custom JsonConverter<string>

* Good, because it's centralized and requires no changes to tool methods
* Good, because it's transparent to the parameter marshalling layer
* Good, because it preserves type safety for valid inputs
* Bad, because it has global side effects on all string deserialization
* Bad, because it might mask client bugs by silently correcting them

### Option B: Request Filter Normalization

* Good, because it's centralized and intercepts at the MCP protocol level
* Good, because it can log warnings when normalization occurs
* Good, because it doesn't affect other parts of the application
* Bad, because it requires schema knowledge to know which parameters should be strings
* Bad, because it's fragile if the MCP SDK changes how arguments are passed

### Option C: Change Parameter Types to JsonElement

* Good, because it provides full control over deserialization per parameter
* Good, because it doesn't have global side effects
* Bad, because it requires changes to every string parameter (23 parameters across 4 tools)
* Bad, because it's verbose and loses automatic type safety
* Bad, because it duplicates deserialization logic across methods

### Option D: Improve Error Handling

* Good, because it's simple and doesn't change behavior
* Good, because it provides clear error messages to guide clients
* Bad, because it doesn't actually fix the problem, just improves the error message
* Bad, because clients still need to retry with correct types
* Bad, because it wastes compute resources on failed requests

## Implementation Details

The MCP SDK v1.4.1's `AIFunctionFactoryOptions` does not expose a `JsonSerializerOptions` property, so a direct `JsonConverter<string>` registration in the parameter marshaller's options is not possible.

**Actual implementation**: A `McpArgumentNormalizationFilter` (MCP request filter) intercepts `CallToolRequestParams` before they reach the tool method. The filter:
1. Iterates over the `Arguments` dictionary (`IDictionary<string, JsonElement>`)
2. For each value with `ValueKind == Array`, extracts elements and joins them with newlines
3. Replaces the array value with a string `JsonElement`
4. Logs a warning when coercion occurs to aid debugging

A standalone `CoerciveStringJsonConverter` class is also provided in `Serialization/` for potential reuse in other JSON deserialization contexts.

The filter is registered before the logging filter in the MCP pipeline, so logged arguments reflect the normalized values.

## Validation & Compliance

* **Unit Tests**: Existing 193 unit tests pass
* **Logging**: Warnings are logged when coercion occurs
* **Build Verification**: `dotnet build` succeeds with 0 errors
* **Lint Check**: `dotnet format --verify-no-changes` passes with 0 errors
