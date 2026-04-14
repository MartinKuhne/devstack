import { logger } from '../observability/logger.js';

export function isDryRunMode(): boolean {
  return process.env.DRY_RUN === 'true';
}

export function getDryRunPlannerResponse(featureTitle: string): string {
  return JSON.stringify({
    plan: `Dry-run: Simulated planning for feature "${featureTitle}"`,
    tasks: [
      {
        title: 'Dry-run task 1',
        deliverable: 'Simulated deliverable 1',
        acceptanceCriteria: 'Dry-run acceptance criteria',
        risks: 'None',
        complexityRating: 3,
        requiredFollowUps: '',
      },
      {
        title: 'Dry-run task 2',
        deliverable: 'Simulated deliverable 2',
        acceptanceCriteria: 'Dry-run acceptance criteria',
        risks: 'None',
        complexityRating: 2,
        requiredFollowUps: '',
      },
    ],
    openQuestions: [],
    securityImpact: 'None',
    performanceImpact: 'None',
    testPlan: 'Standard testing procedures',
    deploymentPlan: 'Standard deployment process',
  });
}

export function getDryRunDevLeadResponse(branchName: string): string {
  return JSON.stringify({
    branchName,
    actions: [
      {
        type: 'create_branch',
        details: `Dry-run: Would create branch "${branchName}"`,
      },
      {
        type: 'transition_status',
        details: 'Dry-run: Would transition feature status',
      },
    ],
  });
}

export function getDryRunCoderResponse(taskTitle: string): string {
  return JSON.stringify({
    summary: `Dry-run: Simulated coding for task "${taskTitle}"`,
    filesModified: ['src/example.ts', 'src/example.test.ts'],
    testResults: 'Dry-run: All tests would pass',
    followUpNeeded: false,
  });
}

export function getDryRunTesterResponse(): string {
  return JSON.stringify({
    buildSuccess: true,
    buildOutput: 'Dry-run: Build would succeed',
    testSuccess: true,
    testOutput: 'Dry-run: All tests would pass',
    failedTests: [],
  });
}

export function getDryRunArchitectResponse(projectId: string): string {
  return JSON.stringify({
    recommendations: [
      {
        type: 'refactoring',
        description: `Dry-run: Consider refactoring module X in project ${projectId}`,
        priority: 'medium',
      },
      {
        type: 'observability',
        description: 'Dry-run: Add more structured logging',
        priority: 'low',
      },
    ],
    memoryUpdate: 'Dry-run: No memory update needed',
    suggestedFeatures: [],
  });
}

export function logDryRunEvent(workflowName: string): void {
  logger.info({ workflow: workflowName, dryRun: true }, 'Dry-run mode: Short-circuiting LLM call');
}
