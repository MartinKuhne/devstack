import { describe, it, expect, vi } from "vitest";
import {
  parseJsonOutput,
  parseJsonOutputWithFallback,
  plannerOutputSchema,
  coderOutputSchema,
  testerOutputSchema,
  devLeadOutputSchema,
  architectOutputSchema,
  createPlannerFallbackParser,
  createCoderFallbackParser,
} from "./output-parser.js";
import { z } from "zod";

describe("parseJsonOutput", () => {
  it("should successfully parse valid JSON output", async () => {
    const schema = z.object({
      name: z.string(),
      value: z.number(),
    });

    const rawOutput = JSON.stringify({ name: "test", value: 42 });

    const result = await parseJsonOutput(rawOutput, schema);

    expect(result.success).toBe(true);
    expect(result.data).toEqual({ name: "test", value: 42 });
    expect(result.retryable).toBe(false);
  });

  it("should handle JSON wrapped in markdown code blocks", async () => {
    const schema = z.object({
      tasks: z.array(z.string()),
    });

    const rawOutput = `Here is the result:
\`\`\`json
{
  "tasks": ["task1", "task2"]
}
\`\`\`
`;

    const result = await parseJsonOutput(rawOutput, schema);

    expect(result.success).toBe(true);
    expect(result.data).toEqual({ tasks: ["task1", "task2"] });
  });

  it("should handle JSON without language specifier in code blocks", async () => {
    const schema = z.object({
      data: z.string(),
    });

    const rawOutput = `\`\`\`
{ "data": "value" }
\`\`\``;

    const result = await parseJsonOutput(rawOutput, schema);

    expect(result.success).toBe(true);
    expect(result.data).toEqual({ data: "value" });
  });

  it("should fail and return retryable error for invalid JSON", async () => {
    const schema = z.object({
      name: z.string(),
    });

    const rawOutput = "{ invalid json }";

    const result = await parseJsonOutput(rawOutput, schema, { maxRetries: 1, retryDelayMs: 10 });

    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
    expect(result.retryable).toBe(true);
  });

  it("should fail and return retryable error for JSON that fails schema validation", async () => {
    const schema = z.object({
      name: z.string(),
      value: z.number(),
    });

    const rawOutput = JSON.stringify({ name: "test", value: "not a number" });

    const result = await parseJsonOutput(rawOutput, schema, { maxRetries: 1, retryDelayMs: 10 });

    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
    expect(result.retryable).toBe(true);
  });

  it("should retry on parse failure up to maxRetries", async () => {
    const schema = z.object({
      value: z.number(),
    });

    const rawOutput = "not json at all";

    const startTime = Date.now();
    const result = await parseJsonOutput(rawOutput, schema, { maxRetries: 2, retryDelayMs: 50 });
    const elapsed = Date.now() - startTime;

    expect(result.success).toBe(false);
    expect(elapsed).toBeGreaterThanOrEqual(150); // 50ms + 100ms (exponential backoff)
  });

  it("should parse planner output schema", async () => {
    const rawOutput = JSON.stringify({
      plan: "This is the plan",
      tasks: [
        {
          title: "Task 1",
          deliverable: "Deliverable 1",
          acceptanceCriteria: "Criteria 1",
          risks: "Risk 1",
          complexityRating: 5,
        },
      ],
      openQuestions: [],
      securityImpact: "None",
      performanceImpact: "None",
      testPlan: "Standard testing",
      deploymentPlan: "Standard deployment",
    });

    const result = await parseJsonOutput(rawOutput, plannerOutputSchema);

    expect(result.success).toBe(true);
    expect(result.data?.tasks).toHaveLength(1);
    expect(result.data?.tasks[0].title).toBe("Task 1");
  });

  it("should parse coder output schema", async () => {
    const rawOutput = JSON.stringify({
      filesModified: ["src/file1.ts", "src/file2.ts"],
      filesCreated: ["src/new.ts"],
      buildResult: "SUCCESS",
      testResult: "SUCCESS",
      summary: "Completed",
    });

    const result = await parseJsonOutput(rawOutput, coderOutputSchema);

    expect(result.success).toBe(true);
    expect(result.data?.filesModified).toEqual(["src/file1.ts", "src/file2.ts"]);
    expect(result.data?.filesCreated).toEqual(["src/new.ts"]);
  });
});

describe("parseJsonOutputWithFallback", () => {
  it("should use JSON parser when valid JSON is provided", async () => {
    const schema = z.object({
      value: z.string(),
    });

    const fallbackParser = vi.fn(() => ({ value: "fallback" }));

    const rawOutput = JSON.stringify({ value: "json" });

    const result = await parseJsonOutputWithFallback(rawOutput, schema, fallbackParser, {
      maxRetries: 1,
      retryDelayMs: 10,
    });

    expect(result.success).toBe(true);
    expect(result.data?.value).toBe("json");
    expect(fallbackParser).not.toHaveBeenCalled();
  });

  it("should use fallback parser when JSON parsing fails", async () => {
    const schema = z.object({
      value: z.string(),
    });

    const fallbackParser = vi.fn(() => ({ value: "fallback result" }));

    const rawOutput = "not json but fallback works";

    const result = await parseJsonOutputWithFallback(rawOutput, schema, fallbackParser, {
      maxRetries: 1,
      retryDelayMs: 10,
    });

    expect(result.success).toBe(true);
    expect(result.data?.value).toBe("fallback result");
    expect(fallbackParser).toHaveBeenCalled();
  });

  it("should return failure when both JSON and fallback parsing fail", async () => {
    const schema = z.object({
      value: z.string().min(10),
    });

    const fallbackParser = vi.fn(() => ({ value: "short" }));

    const rawOutput = "invalid input";

    const result = await parseJsonOutputWithFallback(rawOutput, schema, fallbackParser, {
      maxRetries: 1,
      retryDelayMs: 10,
    });

    expect(result.success).toBe(false);
    expect(result.retryable).toBe(true);
  });
});

describe("Fallback parsers", () => {
  describe("createPlannerFallbackParser", () => {
    it("should extract plan and tasks from markdown format", () => {
      const parser = createPlannerFallbackParser();

      const rawOutput = `## Plan
This is the implementation plan.

## Tasks
- [ ] Implement feature A
  - Deliverable: Complete feature A
  - Acceptance Criteria: Meets requirements
  - Risks: Low
  - Complexity: 5
- [ ] Test feature A
  - Deliverable: Test coverage
  - Acceptance Criteria: 80% coverage
  - Risks: None
  - Complexity: 3

## Open Questions
- What about edge cases?
- Performance requirements?

## Security Impact
No security concerns identified.

## Performance Impact
Minimal performance impact expected.

## Test Plan
Standard unit and integration testing.

## Deployment Plan
Standard deployment process applies.`;

      const result = parser(rawOutput);

      expect(result.plan).toBe("This is the implementation plan.");
      expect(result.tasks).toHaveLength(2);
      expect(result.tasks[0].title).toBe("Implement feature A");
      expect(result.tasks[0].deliverable).toBe("Complete feature A");
      expect(result.tasks[0].complexityRating).toBe(5);
      expect(result.openQuestions).toEqual(["What about edge cases?", "Performance requirements?"]);
    });

    it("should provide defaults when parsing fails", () => {
      const parser = createPlannerFallbackParser();

      const rawOutput = "Just some random text without structure";

      const result = parser(rawOutput);

      expect(result.tasks).toHaveLength(1);
      expect(result.tasks[0].title).toBe("Implement feature");
      expect(result.plan).toBe("");
    });
  });

  describe("createCoderFallbackParser", () => {
    it("should extract file changes from markdown format", () => {
      const parser = createCoderFallbackParser();

      const rawOutput = `## Files Modified
- src/service.ts
- src/handler.ts

## Files Created
- src/new-feature.ts

## Summary
Implementation completed successfully.`;

      const result = parser(rawOutput);

      expect(result.filesModified).toEqual(["src/service.ts", "src/handler.ts"]);
      expect(result.filesCreated).toEqual(["src/new-feature.ts"]);
      expect(result.summary).toBe("Implementation completed successfully.");
    });

    it("should provide defaults when no files are listed", () => {
      const parser = createCoderFallbackParser();

      const rawOutput = "Just some text without file information";

      const result = parser(rawOutput);

      expect(result.filesModified).toEqual(["src/example.ts"]);
      expect(result.filesCreated).toEqual([]);
    });
  });
});

describe("Schema validation", () => {
  it("should validate tester output schema", () => {
    const validOutput = {
      testCases: [
        {
          title: "Test case 1",
          description: "Description 1",
          steps: ["Step 1", "Step 2"],
          expected: "Expected result",
        },
      ],
      summary: "Testing completed",
    };

    const result = testerOutputSchema.safeParse(validOutput);
    expect(result.success).toBe(true);
  });

  it("should validate dev lead output schema", () => {
    const validOutput = {
      assessment: "Feature requires coding work",
      recommendedAction: "assign_to_coder",
      rationale: "Implementation needed",
      priority: "high",
    };

    const result = devLeadOutputSchema.safeParse(validOutput);
    expect(result.success).toBe(true);
  });

  it("should validate architect output schema", () => {
    const validOutput = {
      reviewSummary: "Architecture review completed",
      improvements: [
        {
          title: "Improve error handling",
          description: "Add centralized error handling",
          priority: "high",
          category: "architecture",
        },
      ],
      followUpTasks: [
        {
          title: "Add error handler",
          description: "Implement centralized error handling",
        },
      ],
    };

    const result = architectOutputSchema.safeParse(validOutput);
    expect(result.success).toBe(true);
  });

  it("should reject invalid schema data", () => {
    const invalidOutput = {
      assessment: "Test",
      recommendedAction: "invalid_action",
    };

    const result = devLeadOutputSchema.safeParse(invalidOutput);
    expect(result.success).toBe(false);
  });
});
