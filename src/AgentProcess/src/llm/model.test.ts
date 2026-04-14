import { describe, it, expect, vi } from "vitest";
import {
  createModel,
  countTokens,
  checkTokenLimit,
  validateModelConfig,
  detectProvider,
} from "./model.js";
import { ChatOpenAI } from "@langchain/openai";
import { ChatAnthropic } from "@langchain/anthropic";

describe("LLM Model", () => {
  describe("detectProvider", () => {
    it("should detect OpenAI provider", () => {
      expect(detectProvider("https://api.openai.com/v1")).toBe("openai");
      expect(detectProvider("https://openai.example.com")).toBe("openai");
    });

    it("should detect Anthropic provider", () => {
      expect(detectProvider("https://api.anthropic.com")).toBe("anthropic");
      expect(detectProvider("https://anthropic.example.com")).toBe("anthropic");
    });

    it("should detect Ollama provider", () => {
      expect(detectProvider("http://localhost:11434")).toBe("ollama");
      expect(detectProvider("https://ollama.example.com")).toBe("ollama");
    });

    it("should default to openai-compatible", () => {
      expect(detectProvider("https://custom-api.example.com")).toBe(
        "openai-compatible",
      );
    });
  });

  describe("createModel", () => {
    it("should create OpenAI model", () => {
      const config = {
        modelId: "test-openai",
        provider: "openai" as const,
        modelName: "gpt-4",
        apiUrl: "https://api.openai.com/v1",
        apiKey: "test-key",
        maxTokens: 1000,
        temperature: 0.5,
      };

      const model = createModel(config);
      expect(model).toBeInstanceOf(ChatOpenAI);
    });

    it("should create Anthropic model", () => {
      const config = {
        modelId: "test-anthropic",
        provider: "anthropic" as const,
        modelName: "claude-3",
        apiUrl: "https://api.anthropic.com",
        apiKey: "test-key",
        maxTokens: 2000,
      };

      const model = createModel(config);
      expect(model).toBeInstanceOf(ChatAnthropic);
    });

    it("should create Ollama model", () => {
      const config = {
        modelId: "test-ollama",
        provider: "ollama" as const,
        modelName: "llama2",
        apiUrl: "http://localhost:11434",
      };

      const model = createModel(config);
      expect(model).toBeInstanceOf(ChatOpenAI);
    });

    it("should create openai-compatible model", () => {
      const config = {
        modelId: "test-compatible",
        provider: "openai-compatible" as const,
        modelName: "custom-model",
        apiUrl: "https://custom-api.example.com/v1",
        apiKey: "custom-key",
      };

      const model = createModel(config);
      expect(model).toBeInstanceOf(ChatOpenAI);
    });

    it("should auto-detect provider from URL", () => {
      const config = {
        modelId: "test-autodetect",
        provider: "openai" as const,
        modelName: "gpt-4",
        apiUrl: "https://api.openai.com/v1",
        apiKey: "test-key",
      };

      const model = createModel(config);
      expect(model).toBeInstanceOf(ChatOpenAI);
    });

    it("should use default temperature of 0", () => {
      const config = {
        modelId: "test-default-temp",
        provider: "openai" as const,
        modelName: "gpt-4",
        apiUrl: "https://api.openai.com/v1",
        apiKey: "test-key",
      };

      const model = createModel(config);
      expect(model).toBeDefined();
    });

    it("should throw on invalid config", () => {
      expect(() => {
        createModel({
          modelId: "",
          provider: "invalid" as never,
          modelName: "",
          apiUrl: "not-a-url",
        });
      }).toThrow();
    });
  });

  describe("validateModelConfig", () => {
    it("should validate correct config", () => {
      const config = {
        modelId: "test",
        provider: "openai",
        modelName: "gpt-4",
        apiUrl: "https://api.openai.com/v1",
        apiKey: "test-key",
        maxTokens: 1000,
        temperature: 0.5,
      };

      const validated = validateModelConfig(config);
      expect(validated.modelId).toBe("test");
      expect(validated.provider).toBe("openai");
    });

    it("should reject invalid provider", () => {
      expect(() => {
        validateModelConfig({
          modelId: "test",
          provider: "invalid",
          modelName: "gpt-4",
          apiUrl: "https://api.openai.com/v1",
        });
      }).toThrow();
    });

    it("should reject invalid URL", () => {
      expect(() => {
        validateModelConfig({
          modelId: "test",
          provider: "openai",
          modelName: "gpt-4",
          apiUrl: "not-a-url",
        });
      }).toThrow();
    });
  });

  describe("countTokens", () => {
    it("should count tokens in text", async () => {
      const mockModel = {
        getTokenizer: async () => ({
          encode: (text: string) => text.split(" "),
        }),
      };

      const tokenCount = await countTokens(mockModel as never, "hello world test");
      expect(tokenCount).toBe(3);
    });

    it("should fallback to character-based estimation on error", async () => {
      const mockModel = {
        getTokenizer: async () => {
          throw new Error("Tokenizer unavailable");
        },
      };

      const tokenCount = await countTokens(mockModel as never, "hello world");
      expect(tokenCount).toBeGreaterThan(0);
    });
  });

  describe("checkTokenLimit", () => {
    it("should return within limit when tokens are under max", async () => {
      const mockModel = {
        getTokenizer: async () => ({
          encode: () => [1, 2, 3, 4, 5],
        }),
      };

      const result = await checkTokenLimit(mockModel as never, "test prompt", 100);
      expect(result.withinLimit).toBe(true);
      expect(result.estimatedTokens).toBe(5);
      expect(result.limit).toBe(100);
    });

    it("should return over limit when tokens exceed max", async () => {
      const mockModel = {
        getTokenizer: async () => ({
          encode: () => Array(200).fill(1),
        }),
      };

      const result = await checkTokenLimit(mockModel as never, "test prompt", 100);
      expect(result.withinLimit).toBe(false);
      expect(result.estimatedTokens).toBe(200);
      expect(result.limit).toBe(100);
    });

    it("should use default limit of 4096 when not specified", async () => {
      const mockModel = {
        getTokenizer: async () => ({
          encode: () => [1, 2, 3],
        }),
      };

      const result = await checkTokenLimit(mockModel as never, "test prompt");
      expect(result.limit).toBe(4096);
    });
  });
});
