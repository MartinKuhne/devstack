import { type Job } from 'bullmq';
import { SpanStatusCode } from '@opentelemetry/api';
import { getTracer, workflowMetrics } from './telemetry.js';
import { logger } from './logger.js';

export interface TelemetryOptions {
  workflowType: string;
}

export async function withWorkflowTelemetry<T = unknown>(
  job: Job<T>,
  options: TelemetryOptions,
  processor: () => Promise<unknown>
): Promise<unknown> {
  const tracer = getTracer();
  const span = tracer.startSpan(`workflow.${options.workflowType}`, {
    attributes: {
      'workflow.type': options.workflowType,
      'job.id': job.id ?? 'unknown',
    },
  });

  const startTime = Date.now();

  try {
    workflowMetrics.workflowRunCounter.add(1, {
      workflow_type: options.workflowType,
    });

    logger.info({ jobId: job.id, workflowType: options.workflowType }, 'Starting workflow');

    const result = await processor();

    const duration = Date.now() - startTime;
    span.setStatus({ code: SpanStatusCode.OK });
    span.end();

    workflowMetrics.workflowDurationHistogram.record(duration, {
      workflow_type: options.workflowType,
    });

    logger.info(
      { jobId: job.id, workflowType: options.workflowType, duration },
      'Workflow completed'
    );

    return result;
  } catch (error) {
    const duration = Date.now() - startTime;
    span.setStatus({ code: SpanStatusCode.ERROR, message: String(error) });
    span.end();

    workflowMetrics.workflowDurationHistogram.record(duration, {
      workflow_type: options.workflowType,
    });

    workflowMetrics.workflowFailuresCounter.add(1, {
      workflow_type: options.workflowType,
      error: error instanceof Error ? error.name : 'Unknown',
    });

    logger.error({ jobId: job.id, workflowType: options.workflowType, error }, 'Workflow failed');

    throw error;
  }
}
