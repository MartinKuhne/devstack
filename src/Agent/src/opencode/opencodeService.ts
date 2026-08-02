import { createOpencodeClient, OpencodeClient } from '@opencode-ai/sdk';
import { logger } from '../logger.js';

export interface OpenCodeServiceOptions {
  baseUrl?: string;
  directory?: string;
}

/**
 * OpenCode SDK client wrapper.
 */
export class OpenCodeService {
  private client: OpencodeClient;

  /**
   * Initializes OpenCode SDK service.
   */
  constructor(options: OpenCodeServiceOptions = {}) {
    this.client = createOpencodeClient({
      baseUrl: options.baseUrl || process.env.OPENCODE_BASE_URL || 'http://localhost:4096',
      directory: options.directory,
    });
    logger.debug({ baseUrl: options.baseUrl }, 'Initialized OpenCodeService');
  }

  /**
   * Returns underlying OpenCode SDK client.
   */
  public getClient(): OpencodeClient {
    return this.client;
  }
}
