import * as fs from "node:fs";
import * as path from "node:path";
import { fileURLToPath } from "node:url";
import Handlebars from "handlebars";
import { z } from "zod";
import {
  PlannerPromptOutputSchema,
  DevLeadPromptOutputSchema,
  CoderPromptOutputSchema,
  TesterPromptOutputSchema,
  ArchitectPromptOutputSchema,
  type PlannerPromptInput,
  type DevLeadPromptInput,
  type CoderPromptInput,
  type TesterPromptInput,
  type ArchitectPromptInput,
} from "./types.js";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

let PROMPTS_DIR = path.join(__dirname, "../../prompts");

export function setPromptsDir(dir: string): void {
  PROMPTS_DIR = dir;
}

export function getPromptsDir(): string {
  return PROMPTS_DIR;
}

interface PromptDefinition<TInput, TOutput> {
  template: Handlebars.TemplateDelegate;
  inputSchema: z.ZodSchema<TInput>;
  outputSchema: z.ZodSchema<TOutput>;
  version: string;
}

interface CachedPrompt {
  template: Handlebars.TemplateDelegate;
  version: string;
  lastModified: number;
}

const promptCache = new Map<string, CachedPrompt>();

const outputSchemas: Record<string, z.ZodSchema> = {
  planner: PlannerPromptOutputSchema,
  devlead: DevLeadPromptOutputSchema,
  coder: CoderPromptOutputSchema,
  tester: TesterPromptOutputSchema,
  architect: ArchitectPromptOutputSchema,
};

function findPromptFile(name: string, version?: string): {
  filePath: string;
  version: string;
} | null {
  if (version) {
    const versionedPath = path.join(PROMPTS_DIR, `${name}.v${version}.hbs`);
    if (fs.existsSync(versionedPath)) {
      return { filePath: versionedPath, version };
    }
  }

  const latestPattern = path.join(PROMPTS_DIR, `${name}.v*.hbs`);
  const files = fs.readdirSync(PROMPTS_DIR);
  const matchingFiles = files.filter((f) => {
    const regex = new RegExp(`^${name}\\.v(\\d+)\\.hbs$`);
    return regex.test(f);
  });

  if (matchingFiles.length === 0) {
    return null;
  }

  const latestFile = matchingFiles.reduce((latest, current) => {
    const latestMatch = latest.match(/v(\d+)\.hbs$/);
    const currentMatch = current.match(/v(\d+)\.hbs$/);
    const latestVersion = latestMatch ? parseInt(latestMatch[1], 10) : 0;
    const currentVersion = currentMatch ? parseInt(currentMatch[1], 10) : 0;
    return currentVersion > latestVersion ? current : latest;
  });

  const latestVersion = latestFile.match(/v(\d+)\.hbs$/)?.[1] || "1";
  return {
    filePath: path.join(PROMPTS_DIR, latestFile),
    version: latestVersion,
  };
}

function loadPromptTemplate(filePath: string): Handlebars.TemplateDelegate {
  const templateText = fs.readFileSync(filePath, "utf-8");
  return Handlebars.compile(templateText);
}

function getCachedPrompt(
  name: string,
  version?: string,
): CachedPrompt | null {
  const cacheKey = version ? `${name}:v${version}` : name;
  const cached = promptCache.get(cacheKey);

  if (!cached) {
    return null;
  }

  const found = findPromptFile(name, version);
  if (!found) {
    return null;
  }

  const stats = fs.statSync(found.filePath);
  if (stats.mtimeMs !== cached.lastModified) {
    promptCache.delete(cacheKey);
    return null;
  }

  return cached;
}

function cachePrompt(
  name: string,
  version: string,
  template: Handlebars.TemplateDelegate,
  lastModified: number,
): void {
  const cacheKey = `v${version}`;
  promptCache.set(cacheKey, { template, version, lastModified });
}

export function loadPrompt<TInput extends Record<string, unknown>>(
  name: string,
  version?: string,
): {
  template: Handlebars.TemplateDelegate;
  inputSchema: z.ZodSchema<TInput>;
  version: string;
} {
  const cached = getCachedPrompt(name, version);
  if (cached) {
    return {
      template: cached.template,
      inputSchema: z.any() as z.ZodSchema<TInput>,
      version: cached.version,
    };
  }

  const found = findPromptFile(name, version);
  if (!found) {
    throw new Error(
      `Prompt '${name}'${version ? ` version ${version}` : ""} not found. Expected file in ${PROMPTS_DIR}`,
    );
  }

  const stats = fs.statSync(found.filePath);
  const template = loadPromptTemplate(found.filePath);
  cachePrompt(name, found.version, template, stats.mtimeMs);

  return {
    template,
    inputSchema: z.any() as z.ZodSchema<TInput>,
    version: found.version,
  };
}

export function renderPrompt<TInput extends Record<string, unknown>>(
  name: string,
  input: TInput,
  version?: string,
): string {
  const { template } = loadPrompt<TInput>(name, version);
  return template(input);
}

export function parseOutput<TOutput>(
  name: string,
  rawOutput: string,
): TOutput {
  const schema = outputSchemas[name.toLowerCase()];
  if (!schema) {
    throw new Error(`No output schema registered for prompt '${name}'`);
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(rawOutput);
  } catch (error) {
    throw new Error(
      `Failed to parse LLM output as JSON: ${(error as Error).message}`,
    );
  }

  try {
    return schema.parse(parsed) as TOutput;
  } catch (error) {
    throw new Error(
      `Failed to validate LLM output against schema: ${(error as Error).message}`,
    );
  }
}

export function clearPromptCache(): void {
  promptCache.clear();
}

export function getRegisteredPrompts(): string[] {
  if (!fs.existsSync(PROMPTS_DIR)) {
    return [];
  }

  const files = fs.readdirSync(PROMPTS_DIR);
  const prompts = new Set<string>();

  for (const file of files) {
    const match = file.match(/^([^.]+)\.v\d+\.hbs$/);
    if (match) {
      prompts.add(match[1]);
    }
  }

  return Array.from(prompts);
}
