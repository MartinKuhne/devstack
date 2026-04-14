import { GraphQLClient } from 'graphql-request';
import { loadConfig } from '../config.js';

const config = loadConfig();

export function createGraphQLClient(): GraphQLClient {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (config.GRAPHQL_API_TOKEN) {
    headers.Authorization = `Bearer ${config.GRAPHQL_API_TOKEN}`;
  }

  return new GraphQLClient(config.GRAPHQL_API_URL, {
    headers,
  });
}

export function withTraceContext(client: GraphQLClient, traceparent?: string): GraphQLClient {
  if (traceparent) {
    return new GraphQLClient(config.GRAPHQL_API_URL, {
      headers: {
        'Content-Type': 'application/json',
        'traceparent': traceparent,
        ...(config.GRAPHQL_API_TOKEN ? { Authorization: `Bearer ${config.GRAPHQL_API_TOKEN}` } : {}),
      },
    });
  }
  return client;
}
