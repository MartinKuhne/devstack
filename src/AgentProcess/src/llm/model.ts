import { ChatOpenAI } from "@langchain/openai";
import { ChatAnthropic } from "@langchain/anthropic";
import type { BaseChatModel } from "@langchain/core/language_models/chat_models";
import { z } from "zod";

export interface ModelConfiguration {
  id: string;
  provider: "openai" | "anthropic" | "ollama" | "openai-compatible";
  modelName: string;
  apiUrl: string;
  apiKey?: string;
  maxTokens?: number;
  temperature?: number;
  maxComplexity: number;
}

export interface ModelConfig {
  modelId: string;
  provider: "openai" | "anthropic" | "ollama" | "openai-compatible";
  modelName: string;
  apiUrl: string;
  apiKey?: string;
  maxTokens?: number;
  temperature?: number;
}

const modelConfigSchema = z.object({
  modelId: z.string(),
  provider: z.enum(["openai", "anthropic", "ollama", "openai-compatible"]),
  modelName: z.string(),
  apiUrl: z.string().url(),
  apiKey: z.string().optional(),
  maxTokens: z.number().optional(),
  temperature: z.number().optional(),
});

export type ValidatedModelConfig = z.infer<typeof modelConfigSchema>;

export function detectProvider(apiUrl: string): "openai" | "anthropic" | "ollama" | "openai-compatible" {
  const url = apiUrl.toLowerCase();

  if (url.includes("openai")) {
    return "openai";
  }

  if (url.includes("anthropic")) {
    return "anthropic";
  }

  if (url.includes("ollama") || url.includes("localhost:11434")) {
    return "ollama";
  }

  return "openai-compatible";
}

export function createModel(config: ModelConfig): BaseChatModel {
  const validatedConfig = modelConfigSchema.parse(config);
  const provider = validatedConfig.provider ?? detectProvider(validatedConfig.apiUrl);

  const baseOptions = {
    modelName: validatedConfig.modelName,
    temperature: validatedConfig.temperature ?? 0,
    maxTokens: validatedConfig.maxTokens,
  };

  switch (provider) {
    case "openai": {
      return new ChatOpenAI({
        ...baseOptions,
        openAIApiKey: validatedConfig.apiKey,
      });
    }

    case "anthropic": {
      return new ChatAnthropic({
        ...baseOptions,
        anthropicApiKey: validatedConfig.apiKey,
      });
    }

    case "ollama": {
      return new ChatOpenAI({
        ...baseOptions,
        openAIApiKey: "ollama",
        configuration: {
          baseURL: validatedConfig.apiUrl,
        },
      });
    }

    case "openai-compatible": {
      return new ChatOpenAI({
        ...baseOptions,
        openAIApiKey: validatedConfig.apiKey ?? "not-needed",
        configuration: {
          baseURL: validatedConfig.apiUrl,
        },
      });
    }

    default: {
      throw new Error(`Unsupported provider: ${provider}`);
    }
  }
}

export async function countTokens(model: BaseChatModel, text: string): Promise<number> {
  try {
    const anyModel = model as unknown as { getTokenizer: () => Promise<{ encode: (text: string) => number[] }> };
    const tokenizer = await anyModel.getTokenizer?.();
    if (tokenizer) {
      return tokenizer.encode(text).length;
    }
    throw new Error("Tokenizer not available");
  } catch (error) {
    console.warn("Failed to count tokens, estimating based on character length");
    return Math.ceil(text.length / 4);
  }
}

export async function checkTokenLimit(
  model: BaseChatModel,
  prompt: string,
  maxTokens?: number,
): Promise<{ withinLimit: boolean; estimatedTokens: number; limit?: number }> {
  const estimatedTokens = await countTokens(model, prompt);
  const limit = maxTokens ?? 4096;
  const withinLimit = estimatedTokens <= limit;

  return {
    withinLimit,
    estimatedTokens,
    limit,
  };
}

export function validateModelConfig(config: unknown): ValidatedModelConfig {
  return modelConfigSchema.parse(config);
}
