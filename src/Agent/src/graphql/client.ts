import { GraphQLClient } from 'graphql-request';
import { logger } from '../logger.js';
import { GET_PROJECTS, GET_PROJECT_BY_ID, GET_PROJECTS_FOR_RESOLVER } from './queries.js';

export interface DevStackGraphQLClientOptions {
  endpoint: string;
  headers?: Record<string, string>;
}

export interface ProjectDto {
  id: string;
  name: string;
  description?: string | null;
  repository: string;
  deliverables?: DeliverableDto[];
}

export interface DeliverableDto {
  id: string;
  title: string;
  type: string;
  status: string;
  description?: string | null;
}

/**
 * Wrapper for DevStack GraphQL API client using graphql-request.
 */
export class DevStackGraphQLClient {
  private client: GraphQLClient;

  constructor(options: DevStackGraphQLClientOptions) {
    this.client = new GraphQLClient(options.endpoint, {
      headers: options.headers,
    });
    logger.debug({ endpoint: options.endpoint }, 'Initialized GraphQL Client');
  }

  public getClient(): GraphQLClient {
    return this.client;
  }

  /**
   * [AG-180] Queries projects list with specified first limit (default 50).
   */
  public async getProjects(first = 50): Promise<ProjectDto[]> {
    const data = await this.client.request(GET_PROJECTS, { first });
    const nodes = data.projects?.nodes || [];
    return nodes.map((node) => ({
      id: node.id,
      name: node.name,
      description: node.description,
      repository: node.repository,
    }));
  }

  /**
   * [AG-184] Queries a single project by UUID.
   */
  public async getProjectById(id: string): Promise<ProjectDto | null> {
    const data = await this.client.request(GET_PROJECT_BY_ID, { id });
    if (!data.project) return null;
    return {
      id: data.project.id,
      name: data.project.name,
      description: data.project.description,
      repository: data.project.repository,
      deliverables: (data.project.deliverables || []).map((d) => ({
        id: d.id,
        title: d.title,
        type: String(d.type),
        status: String(d.status),
        description: d.description,
      })),
    };
  }

  /**
   * Matches a DevStack project by normalized remote git URL.
   */
  public async findProjectByRepository(normalizedUrl: string): Promise<ProjectDto | null> {
    const data = await this.client.request(GET_PROJECTS_FOR_RESOLVER);
    const nodes = data.projects?.nodes || [];
    const match = nodes.find(
      (p) => p.repository.toLowerCase() === normalizedUrl.toLowerCase()
    );
    if (!match) return null;
    return {
      id: match.id,
      name: match.name,
      description: match.description,
      repository: match.repository,
      deliverables: (match.deliverables || []).map((d) => ({
        id: d.id,
        title: d.title,
        type: String(d.type),
        status: String(d.status),
        description: d.description,
      })),
    };
  }
}
