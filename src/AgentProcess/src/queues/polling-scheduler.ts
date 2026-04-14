import { Redis } from 'ioredis';
import { GraphQLClient } from 'graphql-request';
import { logger } from '../observability/logger.js';
import {
  enqueuePlannerRun,
  enqueueDevleadRun,
  enqueueCoderRun,
  enqueueTesterRun,
  enqueueArchitectRun,
  PlannerJobData,
  DevleadJobData,
  CoderJobData,
  TesterJobData,
  ArchitectJobData,
} from './scheduler.js';
import { loadConfig } from '../config.js';

const config = loadConfig();

interface ActiveRunLock {
  key: string;
  workflowType: 'planner' | 'devlead' | 'coder' | 'tester' | 'architect';
  entityId: string;
}

export class PollingScheduler {
  private redis: Redis;
  private apiClient: GraphQLClient;
  private intervalMs: number;
  private timer: NodeJS.Timeout | null = null;
  private isRunning = false;
  private lockKey = 'scheduler:lock';

  constructor(redis: Redis, apiClient: GraphQLClient, intervalMs = config.SCHEDULER_INTERVAL) {
    this.redis = redis;
    this.apiClient = apiClient;
    this.intervalMs = intervalMs;
  }

  async start(): Promise<void> {
    if (this.timer) {
      logger.warn('Scheduler is already running');
      return;
    }

    logger.info({ intervalMs: this.intervalMs }, 'Starting polling scheduler');

    this.timer = setInterval(() => {
      void this.scheduleCycle();
    }, this.intervalMs);
  }

  async stop(): Promise<void> {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
      logger.info('Stopping polling scheduler');
    }
  }

  private async scheduleCycle(): Promise<void> {
    if (this.isRunning) {
      logger.debug('Scheduler cycle already in progress, skipping');
      return;
    }

    const lockAcquired = await this.acquireLock();
    if (!lockAcquired) {
      logger.debug('Failed to acquire scheduler lock, skipping cycle');
      return;
    }

    try {
      this.isRunning = true;
      await this.runSchedulingChecks();
    } catch (error) {
      logger.error(error, 'Error during scheduler cycle');
    } finally {
      this.isRunning = false;
      await this.releaseLock();
    }
  }

  private async acquireLock(): Promise<boolean> {
    const result = await this.redis.set(this.lockKey, '1', 'EX', 60, 'NX');
    return result === 'OK';
  }

  private async releaseLock(): Promise<void> {
    await this.redis.del(this.lockKey);
  }

  private async runSchedulingChecks(): Promise<void> {
    logger.debug('Running scheduling checks');

    await this.checkPlannerRuns();
    await this.checkDevleadRuns();
    await this.checkCoderRuns();
    await this.checkTesterRuns();
    await this.checkArchitectRuns();

    logger.debug('Scheduler cycle completed');
  }

  private async isRunActive(workflowType: string, entityId: string): Promise<boolean> {
    const lockKey = `workflow:active:${workflowType}:${entityId}`;
    const exists = await this.redis.exists(lockKey);
    return exists === 1;
  }

  private async markRunActive(workflowType: string, entityId: string): Promise<void> {
    const lockKey = `workflow:active:${workflowType}:${entityId}`;
    await this.redis.setex(lockKey, 3600, '1');
  }

  private async checkPlannerRuns(): Promise<void> {
    try {
      const query = `
        query GetProjectsForPlanner {
          projects {
            id
            name
          }
          features(where: { status_in: [TODO, IN_PROGRESS] }) {
            id
            projectId
            status
          }
          tasks(where: { status_in: [TODO, IN_PROGRESS] }) {
            id
            featureId
            status
          }
        }
      `;

      const result: any = await this.apiClient.request(query);
      const projects = result.projects || [];
      const features = result.features || [];
      const tasks = result.tasks || [];

      for (const project of projects) {
        const lockKey = `workflow:active:planner:${project.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: PlannerJobData = {
            projectId: project.id,
          };

          await enqueuePlannerRun(jobData);
          await this.markRunActive('planner', project.id);
          logger.info({ projectId: project.id }, 'Scheduled planner run for project');
        }
      }

      for (const feature of features) {
        const lockKey = `workflow:active:planner:${feature.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: PlannerJobData = {
            projectId: feature.projectId,
            featureId: feature.id,
          };

          await enqueuePlannerRun(jobData);
          await this.markRunActive('planner', feature.id);
          logger.info({ featureId: feature.id }, 'Scheduled planner run for feature');
        }
      }

      for (const task of tasks) {
        const lockKey = `workflow:active:planner:${task.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: PlannerJobData = {
            projectId: task.featureId,
            taskId: task.id,
          };

          await enqueuePlannerRun(jobData);
          await this.markRunActive('planner', task.id);
          logger.info({ taskId: task.id }, 'Scheduled planner run for task');
        }
      }
    } catch (error) {
      logger.error(error, 'Error checking planner runs');
    }
  }

  private async checkDevleadRuns(): Promise<void> {
    try {
      const query = `
        query GetFeaturesForDevlead {
          features(where: { status_in: [PLANNED, READY_FOR_DEVELOPMENT] }) {
            id
            projectId
            status
          }
        }
      `;

      const result: any = await this.apiClient.request(query);
      const features = result.features || [];

      for (const feature of features) {
        const lockKey = `workflow:active:devlead:${feature.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: DevleadJobData = {
            projectId: feature.projectId,
            featureId: feature.id,
          };

          await enqueueDevleadRun(jobData);
          await this.markRunActive('devlead', feature.id);
          logger.info({ featureId: feature.id }, 'Scheduled devlead run for feature');
        }
      }
    } catch (error) {
      logger.error(error, 'Error checking devlead runs');
    }
  }

  private async checkCoderRuns(): Promise<void> {
    try {
      const query = `
        query GetTasksForCoder {
          tasks(where: { status_in: [READY_FOR_DEVELOPMENT, IN_PROGRESS] }) {
            id
            featureId
            projectId
            status
          }
        }
      `;

      const result: any = await this.apiClient.request(query);
      const tasks = result.tasks || [];

      for (const task of tasks) {
        const lockKey = `workflow:active:coder:${task.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: CoderJobData = {
            projectId: task.projectId,
            featureId: task.featureId,
            taskId: task.id,
          };

          await enqueueCoderRun(jobData);
          await this.markRunActive('coder', task.id);
          logger.info({ taskId: task.id }, 'Scheduled coder run for task');
        }
      }
    } catch (error) {
      logger.error(error, 'Error checking coder runs');
    }
  }

  private async checkTesterRuns(): Promise<void> {
    try {
      const query = `
        query GetTasksForTester {
          tasks(where: { status_in: [READY_FOR_TESTING] }) {
            id
            featureId
            projectId
            status
          }
        }
      `;

      const result: any = await this.apiClient.request(query);
      const tasks = result.tasks || [];

      for (const task of tasks) {
        const lockKey = `workflow:active:tester:${task.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: TesterJobData = {
            projectId: task.projectId,
            featureId: task.featureId,
            taskId: task.id,
          };

          await enqueueTesterRun(jobData);
          await this.markRunActive('tester', task.id);
          logger.info({ taskId: task.id }, 'Scheduled tester run for task');
        }
      }
    } catch (error) {
      logger.error(error, 'Error checking tester runs');
    }
  }

  private async checkArchitectRuns(): Promise<void> {
    try {
      const query = `
        query GetTasksForArchitect {
          tasks(where: { status_in: [NEEDS_ARCHITECTURE_REVIEW] }) {
            id
            featureId
            projectId
            status
          }
        }
      `;

      const result: any = await this.apiClient.request(query);
      const tasks = result.tasks || [];

      for (const task of tasks) {
        const lockKey = `workflow:active:architect:${task.id}`;
        const isActive = await this.redis.exists(lockKey);

        if (isActive === 0) {
          const jobData: ArchitectJobData = {
            projectId: task.projectId,
            featureId: task.featureId,
            taskId: task.id,
          };

          await enqueueArchitectRun(jobData);
          await this.markRunActive('architect', task.id);
          logger.info({ taskId: task.id }, 'Scheduled architect run for task');
        }
      }
    } catch (error) {
      logger.error(error, 'Error checking architect runs');
    }
  }
}
