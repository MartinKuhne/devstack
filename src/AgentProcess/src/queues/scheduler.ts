import { plannerQueue, devleadQueue, coderQueue, testerQueue, architectQueue } from './queues.js';

export interface PlannerJobData {
  projectId: string;
  featureId?: string;
  taskId?: string;
}

export interface DevleadJobData {
  projectId: string;
  featureId: string;
}

export interface CoderJobData {
  projectId: string;
  featureId: string;
  taskId: string;
}

export interface TesterJobData {
  projectId: string;
  featureId: string;
  taskId: string;
  defectContext?: string;
}

export interface ArchitectJobData {
  projectId: string;
  featureId: string;
  taskId: string;
}

export async function enqueuePlannerRun(data: PlannerJobData): Promise<void> {
  await plannerQueue.add('planner-run', data);
}

export async function enqueueDevleadRun(data: DevleadJobData): Promise<void> {
  await devleadQueue.add('devlead-run', data);
}

export async function enqueueCoderRun(data: CoderJobData): Promise<void> {
  await coderQueue.add('coder-run', data);
}

export async function enqueueTesterRun(data: TesterJobData): Promise<void> {
  await testerQueue.add('tester-run', data);
}

export async function enqueueArchitectRun(data: ArchitectJobData): Promise<void> {
  await architectQueue.add('architect-run', data);
}
