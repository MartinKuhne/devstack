import { tool as createLangChainTool } from "@langchain/core/tools";
import type { BaseChatModel } from "@langchain/core/language_models/chat_models";
import type { ChatGeneration } from "@langchain/core/outputs";
import { ToolRegistry } from "./tool-registry.js";
import type { ToolContext } from "./tool.js";
import { logger } from "../observability/logger.js";

export interface BoundTool {
  name: string;
  description: string;
  execute: (input: unknown, context: ToolContext) => Promise<string>;
}

export function bindToolsToModel(
  model: BaseChatModel,
  toolRegistry: ToolRegistry,
  context: ToolContext,
): BaseChatModel {
  const tools = toolRegistry.getToolDefinitions();

  const langChainTools = tools.map((tool) => {
    const langChainTool = createLangChainTool(
      async (input: Record<string, unknown>) => {
        const result = await toolRegistry.executeTool(tool.name, input, context);

        if (result.ok) {
          return JSON.stringify(result.output);
        } else {
          const errorMsg = result.error.message;
          logger.warn({ toolName: tool.name, error: result.error }, "Tool execution returned error");
          return `Error: ${errorMsg}`;
        }
      },
      {
        name: tool.name,
        description: tool.description,
        schema: tool.inputSchema,
      },
    );

    return langChainTool;
  });

  if (!model.bindTools) {
  logger.warn("Model does not support bindTools, returning original model");
  return model;
}

return model.bindTools(langChainTools) as BaseChatModel;
}

export interface ToolCallExecutionResult {
  toolName: string;
  args: Record<string, unknown>;
  result: string;
  success: boolean;
}

export async function executeToolCalls(
  toolCalls: Array<{ name: string; args: Record<string, unknown>; id: string }>,
  toolRegistry: ToolRegistry,
  context: ToolContext,
): Promise<ToolCallExecutionResult[]> {
  const results: ToolCallExecutionResult[] = [];

  for (const call of toolCalls) {
    logger.info({ toolName: call.name, args: call.args }, "Executing tool call");

    const result = await toolRegistry.executeTool(call.name, call.args, context);

    let resultString: string;
    let success = true;

    if (result.ok) {
      resultString = JSON.stringify(result.output);
    } else {
      resultString = `Error: ${result.error.message}`;
      success = false;
      logger.warn({ toolName: call.name, error: result.error }, "Tool execution failed");
    }

    results.push({
      toolName: call.name,
      args: call.args,
      result: resultString,
      success,
    });

    logger.info({ toolName: call.name, success }, "Tool call completed");
  }

  return results;
}

export async function runToolCallingLoop(
  model: BaseChatModel,
  messages: Array<{ role: string; content: string }>,
  toolRegistry: ToolRegistry,
  context: ToolContext,
  maxIterations: number = 10,
): Promise<{
  finalContent: string;
  toolExecutions: ToolCallExecutionResult[];
  iterations: number;
}> {
  const allToolExecutions: ToolCallExecutionResult[] = [];
  let currentMessages = [...messages];
  let iterations = 0;

  while (iterations < maxIterations) {
    iterations++;

    const modelWithTools = bindToolsToModel(model, toolRegistry, context);

    const response = await modelWithTools.invoke(currentMessages);

    const aiMessage = response as { tool_calls?: Array<{ name: string; args: Record<string, unknown>; id: string }> };
const toolCalls = aiMessage.tool_calls;

    if (!toolCalls || toolCalls.length === 0) {
      const content = typeof response.content === "string" ? response.content : JSON.stringify(response.content || "");

      return {
        finalContent: content,
        toolExecutions: allToolExecutions,
        iterations,
      };
    }

    const executionResults = await executeToolCalls(toolCalls, toolRegistry, context);
    allToolExecutions.push(...executionResults);

    currentMessages.push({
      role: "assistant",
      content: `Tool calls were made: ${toolCalls.map((tc) => tc.name).join(", ")}`,
    });

    for (const execution of executionResults) {
      currentMessages.push({
        role: "tool",
        content: execution.result,
      });
    }

    const completedTools = executionResults.filter((r) => !r.success);
    if (completedTools.length > 0 && allToolExecutions.length > 0) {
      const allFailed = executionResults.every((r) => !r.success);
      if (allFailed) {
        logger.warn({ iterations }, "All tool calls failed, stopping loop");
        break;
      }
    }
  }

  if (iterations >= maxIterations) {
    logger.warn({ iterations, toolExecutions: allToolExecutions.length }, "Max iterations reached in tool calling loop");
  }

  return {
    finalContent: `Completed after ${iterations} iterations with ${allToolExecutions.length} tool executions`,
    toolExecutions: allToolExecutions,
    iterations,
  };
}

export function createToolCallingContext(
  toolRegistry: ToolRegistry,
  api: ReturnType<typeof import("../api/graphql-client.js").createGraphQLClient>,
  loggerInstance: typeof logger,
): ToolContext {
  return {
    logger: loggerInstance,
    api,
  };
}
