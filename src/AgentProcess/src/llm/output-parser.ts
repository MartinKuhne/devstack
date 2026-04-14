import { z } from "zod";

export interface ParseResult<T> {
  success: boolean;
  data?: T;
  error?: string;
  retryable: boolean;
}

export interface ParseOptions {
  maxRetries?: number;
  retryDelayMs?: number;
}

export async function parseJsonOutput<T>(
  rawOutput: string,
  schema: z.ZodSchema<T>,
  options: ParseOptions = {},
): Promise<ParseResult<T>> {
  const maxRetries = options.maxRetries ?? 3;
  const retryDelayMs = options.retryDelayMs ?? 1000;

  let lastError: string | undefined;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      const trimmedOutput = rawOutput.trim();

      let jsonStr = trimmedOutput;

      const codeBlockMatch = trimmedOutput.match(/```(?:json)?\s*([\s\S]*?)\s*```/);
      if (codeBlockMatch) {
        jsonStr = codeBlockMatch[1].trim();
      }

      const parsed = JSON.parse(jsonStr);

      const validated = schema.parse(parsed);

      return {
        success: true,
        data: validated,
        retryable: false,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Unknown parsing error";

      lastError = errorMessage;

      if (attempt < maxRetries) {
        await new Promise((resolve) => setTimeout(resolve, retryDelayMs * (attempt + 1)));
      }
    }
  }

  return {
    success: false,
    error: lastError,
    retryable: true,
  };
}

export async function parseJsonOutputWithFallback<T>(
  rawOutput: string,
  schema: z.ZodSchema<T>,
  fallbackParser: (raw: string) => T | null,
  options: ParseOptions = {},
): Promise<ParseResult<T>> {
  const jsonResult = await parseJsonOutput(rawOutput, schema, options);

  if (jsonResult.success) {
    return jsonResult;
  }

  try {
    const fallbackResult = fallbackParser(rawOutput);

    if (fallbackResult !== null) {
      const validated = schema.parse(fallbackResult);

      return {
        success: true,
        data: validated,
        retryable: false,
      };
    }
  } catch (error) {
    const fallbackError = error instanceof Error ? error.message : "Fallback parsing failed";
  }

  return {
    success: false,
    error: `JSON parsing failed: ${jsonResult.error}`,
    retryable: true,
  };
}

export const plannerOutputSchema = z.object({
  plan: z.string(),
  tasks: z.array(
    z.object({
      title: z.string(),
      deliverable: z.string(),
      acceptanceCriteria: z.string(),
      risks: z.string(),
      complexityRating: z.number().min(1).max(10),
      requiredFollowUps: z.string().optional(),
    }),
  ),
  openQuestions: z.array(z.string()),
  securityImpact: z.string(),
  performanceImpact: z.string(),
  testPlan: z.string(),
  deploymentPlan: z.string(),
});

export const coderOutputSchema = z.object({
  filesModified: z.array(z.string()),
  filesCreated: z.array(z.string()),
  buildResult: z.string(),
  testResult: z.string(),
  commitMessage: z.string().optional(),
  summary: z.string(),
  issues: z.array(z.string()).optional(),
});

export const testerOutputSchema = z.object({
  testCases: z.array(
    z.object({
      title: z.string(),
      description: z.string(),
      steps: z.array(z.string()),
      expected: z.string(),
    }),
  ),
  summary: z.string(),
  issues: z.array(z.string()).optional(),
});

export const devLeadOutputSchema = z.object({
  assessment: z.string(),
  recommendedAction: z.enum(["assign_to_coder", "assign_to_tester", "assign_to_architect", "escalate"]),
  rationale: z.string(),
  assignedWorkflow: z.string().optional(),
  priority: z.enum(["low", "medium", "high", "critical"]),
});

export const architectOutputSchema = z.object({
  reviewSummary: z.string(),
  improvements: z.array(
    z.object({
      title: z.string(),
      description: z.string(),
      priority: z.enum(["low", "medium", "high", "critical"]),
      category: z.enum(["architecture", "performance", "security", "maintainability", "other"]),
    }),
  ),
  followUpTasks: z.array(
    z.object({
      title: z.string(),
      description: z.string(),
    }),
  ),
});

export function createPlannerFallbackParser(): (raw: string) => {
  plan: string;
  tasks: Array<{
    title: string;
    deliverable: string;
    acceptanceCriteria: string;
    risks: string;
    complexityRating: number;
    requiredFollowUps?: string;
  }>;
  openQuestions: string[];
  securityImpact: string;
  performanceImpact: string;
  testPlan: string;
  deploymentPlan: string;
} {
  return (raw: string) => {
    const lines = raw.split("\n");

    const planLines: string[] = [];
    const tasks: Array<{
      title: string;
      deliverable: string;
      acceptanceCriteria: string;
      risks: string;
      complexityRating: number;
      requiredFollowUps?: string;
    }> = [];
    const openQuestions: string[] = [];
    let currentSection: "plan" | "tasks" | "questions" | "security" | "performance" | "test" | "deployment" | "unknown" = "unknown";
    let planSectionFound = false;
    let currentTask: {
      title?: string;
      deliverable?: string;
      acceptanceCriteria?: string;
      risks?: string;
      complexityRating?: number;
      requiredFollowUps?: string;
    } = {};
    let sectionSecurityImpact = "";
    let sectionPerformanceImpact = "";
    let sectionTestPlan = "";
    let sectionDeploymentPlan = "";

    for (const line of lines) {
      const trimmed = line.trim();

      if (trimmed.toLowerCase().startsWith("## plan")) {
        currentSection = "plan";
        planSectionFound = true;
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## tasks")) {
        currentSection = "tasks";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## open questions")) {
        currentSection = "questions";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## security impact")) {
        currentSection = "security";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## performance impact")) {
        currentSection = "performance";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## test plan")) {
        currentSection = "test";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## deployment plan")) {
        currentSection = "deployment";
        continue;
      }

      switch (currentSection) {
        case "plan":
          if (trimmed && !trimmed.startsWith("-")) planLines.push(trimmed);
          break;

        case "tasks":
          if (trimmed.startsWith("- [ ]") || trimmed.startsWith("- [x]")) {
            if (currentTask.title && currentTask.deliverable) {
              tasks.push({
                title: currentTask.title,
                deliverable: currentTask.deliverable,
                acceptanceCriteria: currentTask.acceptanceCriteria || "",
                risks: currentTask.risks || "",
                complexityRating: currentTask.complexityRating || 5,
                requiredFollowUps: currentTask.requiredFollowUps,
              });
            }
            currentTask = {
              title: trimmed.replace(/^- \[[ x]\]\s*/i, ""),
              deliverable: "",
              acceptanceCriteria: "",
              risks: "",
              complexityRating: 5,
            };
          } else if (trimmed.startsWith("-")) {
            const content = trimmed.substring(1).trim();
            if (content.toLowerCase().startsWith("deliverable:")) {
              currentTask.deliverable = content.substring(12).trim();
            } else if (content.toLowerCase().startsWith("acceptance criteria:")) {
              currentTask.acceptanceCriteria = content.substring(20).trim();
            } else if (content.toLowerCase().startsWith("risks:")) {
              currentTask.risks = content.substring(6).trim();
            } else if (content.toLowerCase().startsWith("complexity:")) {
              const rating = Number.parseInt(content.substring(11).trim(), 10);
              currentTask.complexityRating = Number.isNaN(rating) ? 5 : rating;
            }
          }
          break;

        case "questions":
          if (trimmed.startsWith("-")) {
            openQuestions.push(trimmed.substring(1).trim());
          } else if (trimmed) {
            openQuestions.push(trimmed);
          }
          break;

        case "security":
          sectionSecurityImpact = trimmed;
          break;

        case "performance":
          sectionPerformanceImpact = trimmed;
          break;

        case "test":
          sectionTestPlan = trimmed;
          break;

        case "deployment":
          sectionDeploymentPlan = trimmed;
          break;
      }
    }

    if (currentTask.title && currentTask.deliverable) {
      tasks.push({
        title: currentTask.title,
        deliverable: currentTask.deliverable,
        acceptanceCriteria: currentTask.acceptanceCriteria || "",
        risks: currentTask.risks || "",
        complexityRating: currentTask.complexityRating || 5,
        requiredFollowUps: currentTask.requiredFollowUps,
      });
    }

    return {
      plan: planSectionFound && planLines.length > 0 ? planLines.join("\n") : "",
      tasks: tasks.length > 0 ? tasks : [{ title: "Implement feature", deliverable: "Complete implementation", acceptanceCriteria: "Meets acceptance criteria", risks: "None identified", complexityRating: 5 }],
      openQuestions,
      securityImpact: sectionSecurityImpact || "No security impact identified",
      performanceImpact: sectionPerformanceImpact || "No performance impact identified",
      testPlan: sectionTestPlan || "Standard testing applies",
      deploymentPlan: sectionDeploymentPlan || "Standard deployment process",
    };
  };
}

export function createCoderFallbackParser(): (raw: string) => {
  filesModified: string[];
  filesCreated: string[];
  buildResult: string;
  testResult: string;
  commitMessage?: string;
  summary: string;
  issues?: string[];
} {
  return (raw: string) => {
    const filesModified: string[] = [];
    const filesCreated: string[] = [];
    const lines = raw.split("\n");

    let currentSection: "modified" | "created" | "summary" | null = null;
    const summaryLines: string[] = [];

    for (const line of lines) {
      const trimmed = line.trim();

      if (trimmed.toLowerCase().startsWith("## files modified")) {
        currentSection = "modified";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## files created")) {
        currentSection = "created";
        continue;
      }
      if (trimmed.toLowerCase().startsWith("## summary")) {
        currentSection = "summary";
        continue;
      }

      if (currentSection === "modified" && trimmed.startsWith("-")) {
        filesModified.push(trimmed.substring(1).trim());
      } else if (currentSection === "created" && trimmed.startsWith("-")) {
        filesCreated.push(trimmed.substring(1).trim());
      } else if (currentSection === "summary" && trimmed) {
        summaryLines.push(trimmed);
      }
    }

    return {
      filesModified: filesModified.length > 0 ? filesModified : ["src/example.ts"],
      filesCreated: filesCreated,
      buildResult: "PENDING",
      testResult: "PENDING",
      summary: summaryLines.length > 0 ? summaryLines.join(" ") : "Code changes completed",
    };
  };
}
