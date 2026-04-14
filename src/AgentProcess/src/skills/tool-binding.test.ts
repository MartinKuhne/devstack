import { describe, it, expect, vi, beforeEach } from "vitest";
import { toolRegistry } from "./tool-registry.js";
import { bindToolsToModel, executeToolCalls, runToolCallingLoop } from "./tool-binding.js";
import { z } from "zod";
import type { ToolContext } from "./tool.js";

describe("Tool Binding", () => {
  beforeEach(() => {
    toolRegistry.getToolDefinitions().forEach((tool) => {
      // Clear existing tools for testing
    });
  });

  it("should register and bind a simple tool", () => {
    const testTool = {
      name: "test_tool",
      description: "A test tool",
      inputSchema: z.object({
        input: z.string(),
      }),
      execute: async (input: { input: string }, _ctx: ToolContext) => {
        return { ok: true as const, output: `Processed: ${input.input}` };
      },
    };

    toolRegistry.register(testTool);

    expect(toolRegistry.hasTool("test_tool")).toBe(true);
    expect(toolRegistry.listTools()).toContain("test_tool");
  });

  it("should execute a registered tool successfully", async () => {
    const testTool = {
      name: "echo_tool",
      description: "Echoes input",
      inputSchema: z.object({
        message: z.string(),
      }),
      execute: async (input: { message: string }, _ctx: ToolContext) => {
        return { ok: true as const, output: input.message };
      },
    };

    toolRegistry.register(testTool);

    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const result = await toolRegistry.executeTool("echo_tool", { message: "Hello" }, mockContext);

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.output).toBe("Hello");
    }
  });

  it("should handle tool execution error gracefully", async () => {
    const failingTool = {
      name: "failing_tool",
      description: "A tool that fails",
      inputSchema: z.object({
        shouldFail: z.boolean(),
      }),
      execute: async (input: { shouldFail: boolean }, _ctx: ToolContext) => {
        if (input.shouldFail) {
          return {
            ok: false as const,
            error: { code: "TEST_ERROR", message: "Intentional failure" },
          };
        }
        return { ok: true as const, output: "Success" };
      },
    };

    toolRegistry.register(failingTool);

    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const result = await toolRegistry.executeTool("failing_tool", { shouldFail: true }, mockContext);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe("TEST_ERROR");
    }
  });

  it("should return error for unknown tool", async () => {
    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const result = await toolRegistry.executeTool("nonexistent_tool", {}, mockContext);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe("TOOL_NOT_FOUND");
    }
  });

  it("should validate input schema before execution", async () => {
    const validatedTool = {
      name: "validated_tool",
      description: "A tool with validation",
      inputSchema: z.object({
        number: z.number().min(0).max(100),
      }),
      execute: async (input: { number: number }, _ctx: ToolContext) => {
        return { ok: true as const, output: input.number * 2 };
      },
    };

    toolRegistry.register(validatedTool);

    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const result = await toolRegistry.executeTool("validated_tool", { number: 5 }, mockContext);

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.output).toBe(10);
    }

    const invalidResult = await toolRegistry.executeTool("validated_tool", { number: 150 }, mockContext);

    expect(invalidResult.ok).toBe(false);
  });
});

describe("executeToolCalls", () => {
  it("should execute multiple tool calls in sequence", async () => {
    toolRegistry.register({
      name: "add",
      description: "Add two numbers",
      inputSchema: z.object({
        a: z.number(),
        b: z.number(),
      }),
      execute: async (input: { a: number; b: number }, _ctx: ToolContext) => {
        return { ok: true as const, output: input.a + input.b };
      },
    });

    toolRegistry.register({
      name: "multiply",
      description: "Multiply two numbers",
      inputSchema: z.object({
        a: z.number(),
        b: z.number(),
      }),
      execute: async (input: { a: number; b: number }, _ctx: ToolContext) => {
        return { ok: true as const, output: input.a * input.b };
      },
    });

    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const toolCalls = [
      { name: "add", args: { a: 2, b: 3 }, id: "call1" },
      { name: "multiply", args: { a: 5, b: 6 }, id: "call2" },
    ];

    const results = await executeToolCalls(toolCalls, toolRegistry, mockContext);

    expect(results).toHaveLength(2);
    expect(results[0].toolName).toBe("add");
    expect(results[0].success).toBe(true);
    expect(results[0].result).toBe("5");
    expect(results[1].toolName).toBe("multiply");
    expect(results[1].success).toBe(true);
    expect(results[1].result).toBe("30");
  });

  it("should handle mixed success and failure in tool calls", async () => {
    toolRegistry.register({
      name: "success_tool",
      description: "Always succeeds",
      inputSchema: z.object({}),
      execute: async (_input: {}, _ctx: ToolContext) => {
        return { ok: true as const, output: "Success" };
      },
    });

    toolRegistry.register({
      name: "fail_tool",
      description: "Always fails",
      inputSchema: z.object({}),
      execute: async (_input: {}, _ctx: ToolContext) => {
        return { ok: false as const, error: { code: "FAIL", message: "Failed" } };
      },
    });

    const mockContext: ToolContext = {
      logger: { info: vi.fn(), warn: vi.fn(), error: vi.fn(), debug: vi.fn() } as any,
      api: {} as any,
    };

    const toolCalls = [
      { name: "success_tool", args: {}, id: "call1" },
      { name: "fail_tool", args: {}, id: "call2" },
    ];

    const results = await executeToolCalls(toolCalls, toolRegistry, mockContext);

    expect(results).toHaveLength(2);
    expect(results[0].success).toBe(true);
    expect(results[1].success).toBe(false);
    expect(results[1].result).toContain("Error: Failed");
  });
});
