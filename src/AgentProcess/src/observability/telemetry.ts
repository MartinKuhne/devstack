import { NodeSDK } from '@opentelemetry/sdk-node';
import { ConsoleSpanExporter } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-http';
import { PeriodicExportingMetricReader, ConsoleMetricExporter } from '@opentelemetry/sdk-metrics';
import { HttpInstrumentation } from '@opentelemetry/instrumentation-http';
import { diag, DiagConsoleLogger, DiagLogLevel, trace, metrics } from '@opentelemetry/api';
import { loadConfig } from '../config.js';

const config = loadConfig();

let sdk: NodeSDK | undefined;

export function initializeTelemetry(): void {
  diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.INFO);

  const traceExporter = config.OTLP_ENDPOINT
    ? new OTLPTraceExporter({ url: config.OTLP_ENDPOINT })
    : new ConsoleSpanExporter();

  const metricReader = new PeriodicExportingMetricReader({
    exporter: config.OTLP_ENDPOINT
      ? new OTLPMetricExporter({ url: config.OTLP_ENDPOINT })
      : new ConsoleMetricExporter(),
    exportIntervalMillis: 10000,
  });

  sdk = new NodeSDK({
    serviceName: 'agent-process',
    traceExporter,
    metricReader,
    instrumentations: [new HttpInstrumentation()],
  });

  sdk.start();
}

export function getTracer(): ReturnType<typeof trace.getTracer> {
  return trace.getTracer('agent-process');
}

export function getMeter(): ReturnType<typeof metrics.getMeter> {
  return metrics.getMeter('agent-process');
}

export function createWorkflowMetrics() {
  const meter = getMeter();

  const workflowDurationHistogram = meter.createHistogram('workflow.duration', {
    description: 'Duration of workflow executions in milliseconds',
    unit: 'ms',
  });

  const workflowRunCounter = meter.createCounter('workflow.runs', {
    description: 'Number of workflow runs',
    unit: '1',
  });

  const workflowFailuresCounter = meter.createCounter('workflow.failures', {
    description: 'Number of workflow failures',
    unit: '1',
  });

  return {
    workflowDurationHistogram,
    workflowRunCounter,
    workflowFailuresCounter,
  };
}

const workflowMetrics = createWorkflowMetrics();

export { workflowMetrics };

export async function shutdownTelemetry(): Promise<void> {
  if (sdk) {
    await sdk.shutdown();
  }
}
