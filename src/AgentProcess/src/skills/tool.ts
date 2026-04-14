import { z, ZodSchema, ZodTypeAny } from 'zod';
import { Logger } from 'pino';
import { GraphQLClient } from 'graphql-request';

export interface ToolContext {
  logger: Logger;
  api: GraphQLClient;
  userId?: string;
  metadata?: Record<string, unknown>;
}

export interface ToolError {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

export interface ToolResultSuccess<TOutput = unknown> {
  ok: true;
  output: TOutput;
}

export interface ToolResultFailure {
  ok: false;
  error: ToolError;
}

export type ToolResult<TOutput = unknown> = ToolResultSuccess<TOutput> | ToolResultFailure;

export interface ToolDefinition<TInput extends ZodTypeAny, TOutput = unknown> {
  name: string;
  description: string;
  inputSchema: TInput;
  outputSchema?: ZodSchema<TOutput>;
  execute: (input: z.infer<TInput>, context: ToolContext) => Promise<ToolResult<TOutput>>;
}

export type Tool = ToolDefinition<ZodTypeAny, unknown>;
