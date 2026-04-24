import { NodeSDK } from '@opentelemetry/sdk-node'
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc'
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-grpc'
import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics'
import { diag, DiagConsoleLogger, DiagLogLevel } from '@opentelemetry/api'

diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.ERROR)

const traceExporter = new OTLPTraceExporter()
const metricReader = new PeriodicExportingMetricReader({
  exporter: new OTLPMetricExporter(),
  exportIntervalMillis: 10000,
})

const sdk = new NodeSDK({
  traceExporter,
  metricReader,
})

export function initOpenTelemetry(): void {
  try {
    sdk.start()
    console.log('OpenTelemetry initialized')
  } catch (error) {
    console.error('Failed to initialize OpenTelemetry:', error)
  }
}

export function shutdownOpenTelemetry(): Promise<void> {
  return sdk.shutdown()
}
