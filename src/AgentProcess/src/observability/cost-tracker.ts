import { getMeter } from './telemetry.js';
import { logger } from './logger.js';

export interface CostRecord {
  modelId: string;
  inputTokens: number;
  outputTokens: number;
  workflowType: string;
  timestamp: string;
  totalCost: number;
}

const MODEL_PRICING: Record<string, { inputPerMillion: number; outputPerMillion: number }> = {
  'llama-3.1-70b': { inputPerMillion: 0.5, outputPerMillion: 0.5 },
  'llama-3.1-8b': { inputPerMillion: 0.1, outputPerMillion: 0.1 },
  'gpt-4o': { inputPerMillion: 2.5, outputPerMillion: 10 },
  'gpt-4o-mini': { inputPerMillion: 0.15, outputPerMillion: 0.6 },
  'claude-3-5-sonnet': { inputPerMillion: 3, outputPerMillion: 15 },
  'claude-3-haiku': { inputPerMillion: 0.25, outputPerMillion: 1.25 },
};

function calculateCost(
  modelId: string,
  inputTokens: number,
  outputTokens: number
): number {
  const pricing = MODEL_PRICING[modelId] || MODEL_PRICING['llama-3.1-70b'];
  const inputCost = (inputTokens / 1_000_000) * pricing.inputPerMillion;
  const outputCost = (outputTokens / 1_000_000) * pricing.outputPerMillion;
  return inputCost + outputCost;
}

export function recordWorkflowCost(
  modelId: string,
  inputTokens: number,
  outputTokens: number,
  workflowType: string
): void {
  const totalCost = calculateCost(modelId, inputTokens, outputTokens);
  const timestamp = new Date().toISOString();

  const record: CostRecord = {
    modelId,
    inputTokens,
    outputTokens,
    workflowType,
    timestamp,
    totalCost,
  };

  logger.info(
    {
      modelId,
      inputTokens,
      outputTokens,
      workflowType,
      totalCost,
      timestamp,
    },
    'Workflow cost recorded'
  );

  const meter = getMeter();
  const costHistogram = meter.createHistogram('workflow.cost', {
    description: 'Cost of workflow executions in USD',
    unit: 'USD',
  });

  costHistogram.record(totalCost, {
    workflowType,
    modelId,
  });
}


