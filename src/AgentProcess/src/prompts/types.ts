import { z } from "zod";

export interface PlannerPromptInput {
  projectId: string;
  featureId: string;
  projectMemory: string;
  architecture: string;
  codingStandards: string;
  defectSummaries?: string;
  modelMaxComplexity: number;
}

export interface PlannerTask {
  title: string;
  deliverable: string;
  acceptanceCriteria: string;
  risks: string;
  complexityRating: number;
  requiredFollowUps?: string;
}

export interface PlannerPromptOutput {
  plan: string;
  tasks: PlannerTask[];
  openQuestions: string[];
  securityImpact: string;
  performanceImpact: string;
  testPlan: string;
  deploymentPlan: string;
}

export const PlannerTaskSchema = z.object({
  title: z.string(),
  deliverable: z.string(),
  acceptanceCriteria: z.string(),
  risks: z.string(),
  complexityRating: z.number().min(1).max(10),
  requiredFollowUps: z.string().optional(),
});

export const PlannerPromptOutputSchema = z.object({
  plan: z.string(),
  tasks: z.array(PlannerTaskSchema),
  openQuestions: z.array(z.string()),
  securityImpact: z.string(),
  performanceImpact: z.string(),
  testPlan: z.string(),
  deploymentPlan: z.string(),
});

export interface DevLeadPromptInput {
  featureId: string;
  featureTitle: string;
  featureDescription: string;
  tasks: Array<{
    id: string;
    title: string;
    status: string;
  }>;
  gitProvider: "github" | "gitea";
  repoUrl: string;
}

export interface DevLeadPromptOutput {
  branchName: string;
  actions: Array<{
    type: "create_branch" | "transition_status" | "create_pr";
    details: string;
  }>;
}

export const DevLeadPromptOutputSchema = z.object({
  branchName: z.string(),
  actions: z.array(
    z.object({
      type: z.enum(["create_branch", "transition_status", "create_pr"]),
      details: z.string(),
    }),
  ),
});

export interface CoderPromptInput {
  taskId: string;
  taskTitle: string;
  taskDescription: string;
  deliverable: string;
  acceptanceCriteria: string;
  projectId: string;
  featureId: string;
  projectMemory: string;
  architecture: string;
  codingStandards: string;
  workspacePath: string;
}

export interface CoderPromptOutput {
  summary: string;
  filesModified: string[];
  testResults?: string;
  followUpNeeded: boolean;
  followUpReason?: string;
}

export const CoderPromptOutputSchema = z.object({
  summary: z.string(),
  filesModified: z.array(z.string()),
  testResults: z.string().optional(),
  followUpNeeded: z.boolean(),
  followUpReason: z.string().optional(),
});

export interface TesterPromptInput {
  featureId: string;
  featureTitle: string;
  projectId: string;
  workspacePath: string;
}

export interface TesterPromptOutput {
  buildSuccess: boolean;
  buildOutput: string;
  testSuccess: boolean;
  testOutput: string;
  failedTests: Array<{
    testName: string;
    errorMessage: string;
    severity: "high" | "medium" | "low";
  }>;
}

export const TesterPromptOutputSchema = z.object({
  buildSuccess: z.boolean(),
  buildOutput: z.string(),
  testSuccess: z.boolean(),
  testOutput: z.string(),
  failedTests: z.array(
    z.object({
      testName: z.string(),
      errorMessage: z.string(),
      severity: z.enum(["high", "medium", "low"]),
    }),
  ),
});

export interface ArchitectPromptInput {
  projectId: string;
  projectMemory: string;
  recentFeatures: Array<{
    id: string;
    title: string;
    status: string;
  }>;
  recentDefects: Array<{
    id: string;
    title: string;
    severity: string;
  }>;
  codingStandards: string;
}

export interface ArchitectPromptOutput {
  recommendations: Array<{
    type: "refactoring" | "security" | "observability" | "performance";
    description: string;
    priority: "high" | "medium" | "low";
  }>;
  memoryUpdate: string;
  suggestedFeatures: Array<{
    title: string;
    description: string;
  }>;
}

export const ArchitectPromptOutputSchema = z.object({
  recommendations: z.array(
    z.object({
      type: z.enum(["refactoring", "security", "observability", "performance"]),
      description: z.string(),
      priority: z.enum(["high", "medium", "low"]),
    }),
  ),
  memoryUpdate: z.string(),
  suggestedFeatures: z.array(
    z.object({
      title: z.string(),
      description: z.string(),
    }),
  ),
});

export type PromptInput =
  | PlannerPromptInput
  | DevLeadPromptInput
  | CoderPromptInput
  | TesterPromptInput
  | ArchitectPromptInput;

export type PromptOutput =
  | PlannerPromptOutput
  | DevLeadPromptOutput
  | CoderPromptOutput
  | TesterPromptOutput
  | ArchitectPromptOutput;
