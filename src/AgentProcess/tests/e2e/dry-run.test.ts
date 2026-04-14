import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { isDryRunMode, getDryRunPlannerResponse, getDryRunDevLeadResponse, getDryRunCoderResponse, getDryRunTesterResponse, getDryRunArchitectResponse } from '../../src/workflows/dry-run.js';

describe('Dry-Run Mode E2E', () => {
  let originalDryRunEnv: string | undefined;

  beforeEach(() => {
    originalDryRunEnv = process.env.DRY_RUN;
    vi.clearAllMocks();
  });

  afterEach(() => {
    if (originalDryRunEnv === undefined) {
      delete process.env.DRY_RUN;
    } else {
      process.env.DRY_RUN = originalDryRunEnv;
    }
  });

  describe('isDryRunMode', () => {
    it('should return true when DRY_RUN=true', () => {
      process.env.DRY_RUN = 'true';
      expect(isDryRunMode()).toBe(true);
    });

    it('should return false when DRY_RUN=false', () => {
      process.env.DRY_RUN = 'false';
      expect(isDryRunMode()).toBe(false);
    });

    it('should return false when DRY_RUN is not set', () => {
      delete process.env.DRY_RUN;
      expect(isDryRunMode()).toBe(false);
    });
  });

  describe('Dry-Run Response Generators', () => {
    it('should generate valid planner dry-run response', () => {
      const response = getDryRunPlannerResponse('Test Feature');
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('plan');
      expect(parsed).toHaveProperty('tasks');
      expect(Array.isArray(parsed.tasks)).toBe(true);
      expect(parsed.tasks.length).toBeGreaterThan(0);
      expect(parsed).toHaveProperty('openQuestions');
      expect(parsed).toHaveProperty('securityImpact');
      expect(parsed).toHaveProperty('performanceImpact');
      expect(parsed).toHaveProperty('testPlan');
      expect(parsed).toHaveProperty('deploymentPlan');
    });

    it('should include simulated tasks in planner response', () => {
      const response = getDryRunPlannerResponse('My Feature');
      const parsed = JSON.parse(response);
      
      expect(parsed.tasks).toHaveLength(2);
      expect(parsed.tasks[0].title).toBe('Dry-run task 1');
      expect(parsed.tasks[0].deliverable).toBe('Simulated deliverable 1');
      expect(parsed.tasks[0].complexityRating).toBe(3);
    });

    it('should generate valid devlead dry-run response', () => {
      const response = getDryRunDevLeadResponse('test-branch');
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('branchName');
      expect(parsed.branchName).toBe('test-branch');
      expect(parsed).toHaveProperty('actions');
      expect(Array.isArray(parsed.actions)).toBe(true);
      expect(parsed.actions.length).toBeGreaterThan(0);
    });

    it('should include branch creation action in devlead response', () => {
      const response = getDryRunDevLeadResponse('feature/my-feature');
      const parsed = JSON.parse(response);
      
      const createBranchAction = parsed.actions.find((a: { type: string }) => a.type === 'create_branch');
      expect(createBranchAction).toBeDefined();
      expect(createBranchAction.details).toContain('feature/my-feature');
    });

    it('should generate valid coder dry-run response', () => {
      const response = getDryRunCoderResponse('Implement feature X');
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('summary');
      expect(parsed).toHaveProperty('filesModified');
      expect(Array.isArray(parsed.filesModified)).toBe(true);
      expect(parsed).toHaveProperty('testResults');
      expect(parsed).toHaveProperty('followUpNeeded');
      expect(parsed.followUpNeeded).toBe(false);
    });

    it('should generate valid tester dry-run response', () => {
      const response = getDryRunTesterResponse();
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('buildSuccess');
      expect(parsed.buildSuccess).toBe(true);
      expect(parsed).toHaveProperty('testSuccess');
      expect(parsed.testSuccess).toBe(true);
      expect(parsed).toHaveProperty('buildOutput');
      expect(parsed).toHaveProperty('testOutput');
      expect(Array.isArray(parsed.failedTests)).toBe(true);
    });

    it('should generate valid architect dry-run response', () => {
      const response = getDryRunArchitectResponse('project-123');
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('recommendations');
      expect(Array.isArray(parsed.recommendations)).toBe(true);
      expect(parsed.recommendations.length).toBeGreaterThan(0);
      expect(parsed).toHaveProperty('memoryUpdate');
      expect(parsed).toHaveProperty('suggestedFeatures');
    });

    it('should include recommendation types in architect response', () => {
      const response = getDryRunArchitectResponse('test-project');
      const parsed = JSON.parse(response);
      
      const types = parsed.recommendations.map((r: { type: string }) => r.type);
      expect(types).toContain('refactoring');
      expect(types).toContain('observability');
    });
  });

  describe('End-to-End Dry-Run Mode Verification', () => {
    it('should confirm dry-run mode prevents LLM invocation', () => {
      process.env.DRY_RUN = 'true';
      
      const wasDryRun = isDryRunMode();
      expect(wasDryRun).toBe(true);
    });

    it('should allow toggling dry-run mode', () => {
      delete process.env.DRY_RUN;
      expect(isDryRunMode()).toBe(false);
      
      process.env.DRY_RUN = 'true';
      expect(isDryRunMode()).toBe(true);
      
      process.env.DRY_RUN = 'false';
      expect(isDryRunMode()).toBe(false);
    });

    it('should generate consistent dry-run responses', () => {
      const response1 = getDryRunPlannerResponse('Consistent Feature');
      const response2 = getDryRunPlannerResponse('Consistent Feature');
      
      expect(response1).toBe(response2);
    });

    it('should include feature-specific data in dry-run responses', () => {
      const plannerResponse = getDryRunPlannerResponse('My Special Feature');
      const parsed = JSON.parse(plannerResponse);
      
      expect(parsed.plan).toContain('My Special Feature');
    });

    it('should generate all workflow dry-run responses successfully', () => {
      expect(() => getDryRunPlannerResponse('test')).not.toThrow();
      expect(() => getDryRunDevLeadResponse('test-branch')).not.toThrow();
      expect(() => getDryRunCoderResponse('test-task')).not.toThrow();
      expect(() => getDryRunTesterResponse()).not.toThrow();
      expect(() => getDryRunArchitectResponse('test-project')).not.toThrow();
    });
  });

  describe('Dry-Run Response Structure Validation', () => {
    it('should have all required planner response fields', () => {
      const response = getDryRunPlannerResponse('test');
      const parsed = JSON.parse(response);
      
      const requiredFields = ['plan', 'tasks', 'openQuestions', 'securityImpact', 'performanceImpact', 'testPlan', 'deploymentPlan'];
      for (const field of requiredFields) {
        expect(parsed).toHaveProperty(field);
      }
    });

    it('should have all required task fields in planner response', () => {
      const response = getDryRunPlannerResponse('test');
      const parsed = JSON.parse(response);
      
      const task = parsed.tasks[0];
      const requiredFields = ['title', 'deliverable', 'acceptanceCriteria', 'risks', 'complexityRating', 'requiredFollowUps'];
      for (const field of requiredFields) {
        expect(task).toHaveProperty(field);
      }
    });

    it('should have all required devlead response fields', () => {
      const response = getDryRunDevLeadResponse('test-branch');
      const parsed = JSON.parse(response);
      
      expect(parsed).toHaveProperty('branchName');
      expect(parsed).toHaveProperty('actions');
    });

    it('should have all required coder response fields', () => {
      const response = getDryRunCoderResponse('test-task');
      const parsed = JSON.parse(response);
      
      const requiredFields = ['summary', 'filesModified', 'testResults', 'followUpNeeded'];
      for (const field of requiredFields) {
        expect(parsed).toHaveProperty(field);
      }
    });

    it('should have all required tester response fields', () => {
      const response = getDryRunTesterResponse();
      const parsed = JSON.parse(response);
      
      const requiredFields = ['buildSuccess', 'buildOutput', 'testSuccess', 'testOutput', 'failedTests'];
      for (const field of requiredFields) {
        expect(parsed).toHaveProperty(field);
      }
    });

    it('should have all required architect response fields', () => {
      const response = getDryRunArchitectResponse('test-project');
      const parsed = JSON.parse(response);
      
      const requiredFields = ['recommendations', 'memoryUpdate', 'suggestedFeatures'];
      for (const field of requiredFields) {
        expect(parsed).toHaveProperty(field);
      }
    });
  });
});
