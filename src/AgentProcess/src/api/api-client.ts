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
  totalProjects: number;
  totalFeatures: number;
  totalTasks: number;
  activeWorkflows: number;
  recentActivity: {
    id: string;
    entityType: string;
    action: string;
    timestamp: string;
    description: string;
  }[];
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  githubUrl?: string;
  features?: {
    id: string;
    name: string;
    status: string;
  }[];
}

export interface Feature {
  id: string;
  name: string;
  description?: string;
  status: string;
  projectId: string;
  createdAt: string;
  updatedAt: string;
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  status: string;
  complexity?: number;
  featureId: string;
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
    const response = await this.client.request<{ project: Project }>(getProjectQuery, { id });
    return response.project;
  }

  async getFeatures(projectId: string): Promise<Feature[]> {
    const response = await this.client.request<{ features: Feature[] }>(getFeaturesQuery, {
      projectId,
    });
    return response.features;
  }

  async getTasks(featureId: string): Promise<Task[]> {
    const response = await this.client.request<{ tasks: Task[] }>(getTasksQuery, { featureId });
    return response.tasks;
  }

  async createProject(name: string, description?: string): Promise<Project> {
    const response = await this.client.request<{ createProject: Project }>(createProjectMutation, {
      name,
      description,
    });
    return response.createProject;
  }

  async updateProject(
    id: string,
    name?: string,
    description?: string,
    status?: string
  ): Promise<Project> {
    const response = await this.client.request<{ updateProject: Project }>(updateProjectMutation, {
      id,
      name,
      description,
      status,
    });
    return response.updateProject;
  }

  async createFeature(projectId: string, name: string, description?: string): Promise<Feature> {
    const response = await this.client.request<{ createFeature: Feature }>(createFeatureMutation, {
      projectId,
      name,
      description,
    });
    return response.createFeature;
  }

  async updateFeature(
    id: string,
    name?: string,
    description?: string,
    status?: string
  ): Promise<Feature> {
    const response = await this.client.request<{ updateFeature: Feature }>(updateFeatureMutation, {
      id,
      name,
      description,
      status,
    });
    return response.updateFeature;
  }

  async createTask(
    featureId: string,
    title: string,
    description?: string,
    complexity?: number
  ): Promise<Task> {
    const response = await this.client.request<{ createTask: Task }>(createTaskMutation, {
      featureId,
      title,
      description,
      complexity,
    });
    return response.createTask;
  }

  async updateTask(
    id: string,
    title?: string,
    description?: string,
    status?: string,
    complexity?: number
  ): Promise<Task> {
    const response = await this.client.request<{ updateTask: Task }>(updateTaskMutation, {
      id,
      title,
      description,
      status,
      complexity,
    });
    return response.updateTask;
  }
}
