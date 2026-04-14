import { describe, it, expect, beforeEach, afterEach } from "vitest";
import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";
import {
  loadPrompt,
  renderPrompt,
  parseOutput,
  clearPromptCache,
  getRegisteredPrompts,
  setPromptsDir,
} from "./loader.js";
import {
  PlannerPromptOutputSchema,
  type PlannerPromptInput,
} from "./types.js";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const TEST_PROMPTS_DIR = path.join(__dirname, "../../test-prompts");

describe("Prompt Loader", () => {
  beforeEach(() => {
    clearPromptCache();
    if (!fs.existsSync(TEST_PROMPTS_DIR)) {
      fs.mkdirSync(TEST_PROMPTS_DIR, { recursive: true });
    }
    setPromptsDir(TEST_PROMPTS_DIR);
  });

  afterEach(() => {
    clearPromptCache();
    if (fs.existsSync(TEST_PROMPTS_DIR)) {
      fs.rmSync(TEST_PROMPTS_DIR, { recursive: true, force: true });
    }
  });

  it("should load a simple prompt template", () => {
    const promptContent = `Hello, {{name}}!`;
    fs.writeFileSync(
      path.join(TEST_PROMPTS_DIR, "greeting.v1.hbs"),
      promptContent,
    );

    const { template } = loadPrompt<{ name: string }>("greeting", "1");
    const result = template({ name: "World" });

    expect(result).toBe("Hello, World!");
  });

  it("should render a prompt with input data", () => {
    const promptContent = `Project: {{projectId}}\nFeature: {{featureId}}`;
    fs.writeFileSync(
      path.join(TEST_PROMPTS_DIR, "test.v1.hbs"),
      promptContent,
    );

    const result = renderPrompt(
      "test",
      { projectId: "proj-1", featureId: "feat-1" },
      "1",
    );

    expect(result).toContain("Project: proj-1");
    expect(result).toContain("Feature: feat-1");
  });

  it("should find the latest version when no version specified", () => {
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "test.v1.hbs"), "v1");
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "test.v2.hbs"), "v2");
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "test.v3.hbs"), "v3");

    const { version } = loadPrompt("test");

    expect(version).toBe("3");
  });

  it("should parse valid JSON output", () => {
    const rawOutput = JSON.stringify({
      plan: "Test plan",
      tasks: [
        {
          title: "Task 1",
          deliverable: "Deliverable 1",
          acceptanceCriteria: "Criteria 1",
          risks: "",
          complexityRating: 5,
        },
      ],
      openQuestions: [],
      securityImpact: "None",
      performanceImpact: "None",
      testPlan: "Run tests",
      deploymentPlan: "Deploy",
    });

    const result = parseOutput<{ plan: string; tasks: Array<{ title: string }> }>(
      "planner",
      rawOutput,
    );

    expect(result.plan).toBe("Test plan");
    expect(result.tasks).toHaveLength(1);
    expect(result.tasks[0].title).toBe("Task 1");
  });

  it("should throw on invalid JSON output", () => {
    const rawOutput = "not valid json";

    expect(() => parseOutput("planner", rawOutput)).toThrow(
      "Failed to parse LLM output as JSON",
    );
  });

  it("should throw on invalid schema output", () => {
    const rawOutput = JSON.stringify({
      plan: "Test plan",
      tasks: [
        {
          title: "Task 1",
          deliverable: "Deliverable 1",
          acceptanceCriteria: "Criteria 1",
          risks: "",
          complexityRating: 15,
        },
      ],
      openQuestions: [],
      securityImpact: "None",
      performanceImpact: "None",
      testPlan: "Run tests",
      deploymentPlan: "Deploy",
    });

    expect(() => parseOutput("planner", rawOutput)).toThrow(
      "Failed to validate LLM output against schema",
    );
  });

  it("should cache compiled templates", () => {
    const promptContent = `Cached: {{value}}`;
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "cache.v1.hbs"), promptContent);

    const start = Date.now();
    loadPrompt("cache", "1");
    loadPrompt("cache", "1");
    loadPrompt("cache", "1");
    const duration = Date.now() - start;

    expect(duration).toBeLessThan(10);
  });

  it("should clear the prompt cache", () => {
    const promptContent = `Cache test`;
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "clear2.v1.hbs"), promptContent);

    loadPrompt("clear2", "1");
    const beforeClear = loadPrompt("clear2", "1");

    clearPromptCache();

    const afterClear = loadPrompt("clear2", "1");

    expect(beforeClear).toBeDefined();
    expect(afterClear).toBeDefined();
  });

  it("should list registered prompts from directory", () => {
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "one.v1.hbs"), "one");
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "two.v2.hbs"), "two");
    fs.writeFileSync(path.join(TEST_PROMPTS_DIR, "three.v1.hbs"), "three");

    const prompts = getRegisteredPrompts();

    expect(prompts).toContain("one");
    expect(prompts).toContain("two");
    expect(prompts).toContain("three");
  });
});
