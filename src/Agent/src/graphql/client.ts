import { GraphQLClient } from 'graphql-request';
import { logger } from '../logger.js';

export interface DevStackGraphQLClientOptions {
  endpoint: string;
  headers?: Record<string, string>;
}

/**
 * Wrapper for DevStack GraphQL API client using graphql-request.
 */
export class DevStackGraphQLClient {
  private client: GraphQLClient;

  /**
   * Initializes GraphQLClient with the specified endpoint.
   */
  constructor(options: DevStackGraphQLClientOptions) {
    this.client = new GraphQLClient(options.endpoint, {
      headers: options.headers,
    });
    logger.debug({ endpoint: options.endpoint }, 'Initialized GraphQL Client');
  }

  /**
   * Returns underlying GraphQLClient instance.
   */
  public getClient(): GraphQLClient {
    return this.client;
  }
}
