import { workflowRegistry } from './registry.js';
import { WorkflowContext, WorkflowResult, WorkflowFailureResult, WorkflowError, WorkflowRunStatus, WorkflowEvent } from './types.js';
import { createGraphQLClient } from '../api/graphql-client.js';
import { getTracer } from '../observability/telemetry.js';
import { logger } from '../observability/logger.js';
import { workflowMetrics } from '../observability/telemetry.js';
import { ZodError } from 'zod';

const tracer = getTracer();

interface CreateWorkflowRunInput {
  workflowName: string;
  jobId: string;
  input: unknown;
}

interface UpdateWorkflowRunInput {
  workflowRunId: string;
  status: WorkflowRunStatus;
  outputPayload?: string;
  errorMessage?: string;
}

const mockGraphQLClient = createGraphQLClient();

async function createWorkflowRun(input: CreateWorkflowRunInput): Promise<string> {
  logger.info({ workflowName: input.workflowName, jobId: input.jobId }, 'Creating workflow run record');
  return `workflow-run-${input.jobId}-${Date.now()}`;
}

async function updateWorkflowRun(input: UpdateWorkflowRunInput): Promise<void> {
  logger.info({ 
    workflowRunId: input.workflowRunId, 
    status: input.status,
    errorMessage: input.errorMessage 
  }, 'Updating workflow run record');
}

export async function executeWorkflow<TInput, TOutput>(
  name: string,
  jobId: string,
  input: TInput
): Promise<WorkflowResult<TOutput>> {
  const workflow = workflowRegistry.getWorkflow(name);
  
  const startTime = Date.now();
  let workflowRunId: string | null = null;
  let output: TOutput | null = null;
  let error: WorkflowError | null = null;
  let status: WorkflowRunStatus = 'running';
  const events: WorkflowEvent[] = [];

  const cancel = () => {
    status = 'cancelled';
  };

  try {
    const validatedInput = workflow.inputSchema.parse(input);
    
    workflowRunId = await createWorkflowRun({
      workflowName: name,
      jobId,
      input,
    });

    return await tracer.startActiveSpan(`workflow.${name}`, async (span) => {
      try {
        const ctx: WorkflowContext<TInput, TOutput> = {
          input: validatedInput as TInput,
          api: mockGraphQLClient,
          logger,
          span,
          attempt: 1,
          cancel,
        };

        logger.info({ workflowName: name, jobId, workflowRunId }, 'Executing workflow');
        events.push({
          type: 'workflow_started',
          data: { timestamp: new Date().toISOString() },
          timestamp: new Date().toISOString(),
        });

        const result = await Promise.race([
          workflow.run(ctx),
          new Promise<WorkflowFailureResult>((_, reject) => 
            setTimeout(() => reject(new Error('Workflow timeout')), workflow.timeout)
          )
        ]);

        if (result.ok) {
          status = 'succeeded';
          output = result.output as TOutput;
          events.push(...result.events);
          events.push({
            type: 'workflow_succeeded',
            data: { timestamp: new Date().toISOString() },
            timestamp: new Date().toISOString(),
          });
          
          logger.info({ workflowName: name, jobId, workflowRunId }, 'Workflow completed successfully');
        } else {
          status = 'failed';
          error = result.error;
          events.push(...result.events);
          events.push({
            type: 'workflow_failed',
            data: { 
              timestamp: new Date().toISOString(),
              errorCode: error.code,
              errorMessage: error.message,
              retryable: error.retryable 
            },
            timestamp: new Date().toISOString(),
          });

          logger.error(
            { workflowName: name, jobId, workflowRunId, errorCode: error.code, errorMessage: error.message },
            'Workflow failed'
          );

          if (!error.retryable && workflowRunId) {
            await updateWorkflowRun({
              workflowRunId,
              status: 'failed',
              errorMessage: error.message,
            });
          }

          throw error;
        }

        span.setStatus({ code: 1 });
        span.end();

        const duration = Date.now() - startTime;
        workflowMetrics.workflowDurationHistogram.record(duration, {
          workflowType: name,
          status: 'succeeded',
        });
        workflowMetrics.workflowRunCounter.add(1, {
          workflowType: name,
          status: 'succeeded',
        });

        if (workflowRunId) {
          await updateWorkflowRun({
            workflowRunId,
            status: 'succeeded',
            outputPayload: JSON.stringify(output),
          });
        }

        return result as WorkflowResult<TOutput>;
      } catch (workflowError) {
        if (workflowError instanceof ZodError) {
          const validationError: WorkflowError = {
            code: 'VALIDATION_ERROR',
            message: `Input validation failed: ${workflowError.message}`,
            retryable: false,
            details: { issues: workflowError.issues },
          };

          const zodError = workflowError as ZodError;
          events.push({
            type: 'validation_error',
            data: { 
              timestamp: new Date().toISOString(),
              errors: zodError.issues 
            },
            timestamp: new Date().toISOString(),
          });

          logger.error(
            { workflowName: name, jobId, issues: zodError.issues },
            'Workflow input validation failed'
          );

          span.setStatus({ 
            code: 2, 
            message: validationError.message 
          });
          span.end();

          workflowMetrics.workflowRunCounter.add(1, {
            workflowType: name,
            status: 'failed',
          });

          throw validationError;
        }

        if (workflowError instanceof Error && workflowError.message === 'Workflow timeout') {
          const timeoutError: WorkflowError = {
            code: 'TIMEOUT',
            message: `Workflow ${name} exceeded timeout of ${workflow.timeout}ms`,
            retryable: false,
          };

          events.push({
            type: 'timeout',
            data: { timestamp: new Date().toISOString() },
            timestamp: new Date().toISOString(),
          });

          logger.error({ workflowName: name, jobId, timeoutMs: workflow.timeout }, 'Workflow timed out');

          if (workflowRunId) {
            await updateWorkflowRun({
              workflowRunId,
              status: 'failed',
              errorMessage: timeoutError.message,
            });
          }

          span.setStatus({ 
            code: 2, 
            message: timeoutError.message 
          });
          span.end();

          workflowMetrics.workflowFailuresCounter.add(1, {
            workflowType: name,
          });

          throw timeoutError;
        }

        const workflowErrorTyped = workflowError as WorkflowError;
        const isRetryable = workflowErrorTyped.retryable ?? true;

        if (!isRetryable && workflowRunId) {
          await updateWorkflowRun({
            workflowRunId,
            status: 'failed',
            errorMessage: workflowErrorTyped.message,
          });
        }

        span.setStatus({ 
          code: 2, 
          message: workflowErrorTyped.message 
        });
        span.end();

        workflowMetrics.workflowFailuresCounter.add(1, {
          workflowType: name,
        });

        throw workflowError;
      }
    });
  } catch (execError) {
    const execErrorTyped = execError as WorkflowError;
    
    if (execErrorTyped.code !== 'VALIDATION_ERROR' && execErrorTyped.code !== 'TIMEOUT') {
      workflowMetrics.workflowFailuresCounter.add(1, {
        workflowType: name,
      });
    }

    throw execError;
  }
}
