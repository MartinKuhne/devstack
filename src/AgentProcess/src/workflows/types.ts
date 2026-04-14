import { ZodSchema } from 'zod';
import { Span } from '@opentelemetry/api';
import { GraphQLClient } from 'graphql-request';
import { Logger } from 'pino';

export type WorkflowRunStatus = 'queued' | 'running' | 'succeeded' | 'failed' | 'cancelled';

export interface WorkflowEvent {
  type: string;
  data: unknown;
  timestamp: string;
}

export interface WorkflowError {
  code: string;
  message: string;
  retryable: boolean;
  details?: unknown;
}

export interface WorkflowResult<TOutput> {
  ok: true;
  output: TOutput;
  events: WorkflowEvent[];
}

export interface WorkflowFailureResult {
  ok: false;
  error: WorkflowError;
  events: WorkflowEvent[];
}

export interface WorkflowContext<TInput, TOutput> {
  input: TInput;
  api: GraphQLClient;
  logger: Logger;
  span: Span;
  attempt: number;
  cancel: () => void;
}

export interface WorkflowDefinition<TInput = unknown, TOutput = unknown> {
  name: string;
  inputSchema: ZodSchema<TInput>;
  run: (ctx: WorkflowContext<TInput, TOutput>) => Promise<WorkflowResult<TOutput> | WorkflowFailureResult>;
  maxRetries: number;
  timeout: number;
}
