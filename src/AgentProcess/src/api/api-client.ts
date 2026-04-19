import { GraphQLClient } from 'graphql-request';
import {
  dashboardSummaryQuery,
  getProjectQuery,
  getFeaturesQuery,
  getTasksQuery,
} from './queries/queries.js';
import {
  createProjectMutation,
  updateProjectMutation,
  createFeatureMutation,
  updateFeatureMutation,
  createTaskMutation,
  updateTaskMutation,
} from './mutations/mutations.js';

export interface DashboardSummary {
  projectsInFlight: number;
  featuresInReview: number;
  featuresFailed: number;
  tasksInProgress: number;
  tasksFailed: number;
  recentAuditEvents: {
    id: string;
    entityType: string;
    eventType: string;
    oldValue?: string;
    newValue?: string;
    actor?: string;
    occurredAt: string;
  }[];
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  githubUrl?: string;
  items?: {
    id: string;
    title: string;
    status: string;
  }[];
}

export interface Feature {
  id: string;
  title: string;
  description?: string;
  status: string;
  projectId: string;
  createdAt: string;
  updatedAt: string;
}

export interface Task {
  id: string;
  title: string;
  deliverable?: string;
  status: string;
  complexityRating?: number;
  itemId: string;
  createdAt: string;
  updatedAt: string;
}

export class ApiClient {
  constructor(private client: GraphQLClient) {}

  async getDashboardSummary(): Promise<DashboardSummary> {
    const response = await this.client.request<{ dashboardSummary: DashboardSummary }>(
      dashboardSummaryQuery
    );
    return response.dashboardSummary;
  }

  async getProject(id: string): Promise<Project> {
    const response = await this.client.request<{ projectById: Project }>(getProjectQuery, { id });
    return response.projectById;
  }

  async getFeatures(projectId: string): Promise<Feature[]> {
    const response = await this.client.request<{ items: { nodes: Feature[] } }>(getFeaturesQuery, {
      projectId,
    });
    return response.items.nodes;
  }

  async getTasks(itemId: string): Promise<Task[]> {
    const response = await this.client.request<{ tasks: { nodes: Task[] } }>(getTasksQuery, { itemId });
    return response.tasks.nodes;
  }

  async createProject(name: string, description?: string): Promise<Project> {
    const input = { name, description };
    const response = await this.client.request<{ createProject: { project: Project; errors: string[] } }>(
      createProjectMutation,
      { input }
    );
    if (response.createProject.errors.length > 0) {
      throw new Error(response.createProject.errors.join(', '));
    }
    return response.createProject.project;
  }

  async updateProject(id: string, name?: string, description?: string): Promise<Project> {
    const input = { id, name, description };
    const response = await this.client.request<{ updateProject: { project: Project; errors: string[] } }>(
      updateProjectMutation,
      { input }
    );
    if (response.updateProject.errors.length > 0) {
      throw new Error(response.updateProject.errors.join(', '));
    }
    return response.updateProject.project;
  }

  async createFeature(projectId: string, title: string, description?: string): Promise<Feature> {
    const input = { projectId, title, description };
    const response = await this.client.request<{ createFeature: { item: Feature; errors: string[] } }>(
      createFeatureMutation,
      { input }
    );
    if (response.createFeature.errors.length > 0) {
      throw new Error(response.createFeature.errors.join(', '));
    }
    return response.createFeature.item;
  }

  async updateFeature(id: string, title?: string, description?: string): Promise<Feature> {
    const input = { id, title, description };
    const response = await this.client.request<{ updateFeature: { item: Feature; errors: string[] } }>(
      updateFeatureMutation,
      { input }
    );
    if (response.updateFeature.errors.length > 0) {
      throw new Error(response.updateFeature.errors.join(', '));
    }
    return response.updateFeature.item;
  }

  async createTask(itemId: string, title: string, complexityRating?: number): Promise<Task> {
    const input = { itemId, title, complexityRating };
    const response = await this.client.request<{ createTask: { task: Task; errors: string[] } }>(
      createTaskMutation,
      { input }
    );
    if (response.createTask.errors.length > 0) {
      throw new Error(response.createTask.errors.join(', '));
    }
    return response.createTask.task;
  }

  async updateTask(id: string, title?: string): Promise<Task> {
    const input = { id, title };
    const response = await this.client.request<{ updateTask: { task: Task; errors: string[] } }>(updateTaskMutation, {
      input,
    });
    if (response.updateTask.errors.length > 0) {
      throw new Error(response.updateTask.errors.join(', '));
    }
    return response.updateTask.task;
  }
}
