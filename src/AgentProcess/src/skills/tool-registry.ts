import { Tool, ToolDefinition, ToolContext, ToolResult } from './tool.js';
import { ZodTypeAny } from 'zod';
import { logger } from '../observability/logger.js';

export class ToolRegistry {
  private tools: Map<string, Tool> = new Map();

  register<TInput extends ZodTypeAny, TOutput>(tool: ToolDefinition<TInput, TOutput>): void {
    if (this.tools.has(tool.name)) {
      logger.warn({ toolName: tool.name }, 'Tool already registered, overwriting');
    }

    this.tools.set(tool.name, tool);
    logger.debug({ toolName: tool.name, description: tool.description }, 'Registered tool');
  }

  getTool(name: string): Tool | undefined {
    return this.tools.get(name);
  }

  hasTool(name: string): boolean {
    return this.tools.has(name);
  }

  listTools(): string[] {
    return Array.from(this.tools.keys());
  }

  getToolDefinitions(): Tool[] {
    return Array.from(this.tools.values());
  }

  async executeTool<TOutput>(
    name: string,
    input: unknown,
    context: ToolContext
  ): Promise<ToolResult<TOutput>> {
    const tool = this.getTool(name);

    if (!tool) {
      logger.warn({ toolName: name }, 'Tool not found');
      return {
        ok: false,
        error: {
          code: 'TOOL_NOT_FOUND',
          message: `Tool not found: ${name}`,
        },
      };
    }

    try {
      const parsedInput = tool.inputSchema.parse(input);
      const result = (await tool.execute(parsedInput, context)) as ToolResult<TOutput>;
      return result;
    } catch (error) {
      logger.error({ toolName: name, error }, 'Tool execution failed');

      if (error instanceof Error) {
        return {
          ok: false,
          error: {
            code: 'TOOL_EXECUTION_ERROR',
            message: error.message,
            details: { stack: error.stack },
          },
        };
      }

      return {
        ok: false,
        error: {
          code: 'TOOL_EXECUTION_ERROR',
          message: 'Unknown error during tool execution',
        },
      };
    }
  }

  getToolsForPrompt(): Array<{
    name: string;
    description: string;
    inputSchema: unknown;
  }> {
    return Array.from(this.tools.values()).map((tool) => ({
      name: tool.name,
      description: tool.description,
      inputSchema: (tool.inputSchema as any).shape,
    }));
  }
}

export const toolRegistry = new ToolRegistry();
