import { OpencodeClient, createOpencodeClient } from '@opencode-ai/sdk';
import { logger } from '../logger.js';
import { Agent, setGlobalDispatcher } from 'undici';
import { printMessage, printError, exitProcess, EXIT_CODE_ERROR } from '../cli/output.js';

const FETCH_TIMEOUT_MS = 30 * 60 * 1000;
const TRUNCATE_LENGTH_240 = 240;
const TRUNCATE_LENGTH_160 = 160;

// Increase global fetch headers timeout to 30 minutes (default is 5 minutes)
// This is necessary because LLM prompt executions can take a very long time
setGlobalDispatcher(
  new Agent({
    headersTimeout: FETCH_TIMEOUT_MS,
  })
);

export interface ModelRef {
  providerId: string;
  modelId: string;
}

export interface RunPromptOptions {
  prompt: string;
  title?: string;
  modelProvider?: string;
  modelName?: string;
}

interface ProviderInfo {
  id: string;
  name?: string;
  models?: Record<string, { id: string; name?: string }> | Array<{ id: string; name?: string }>;
}

interface ProvidersResponseData {
  providers?: ProviderInfo[];
  connected?: string[];
}

/**
 * [AG-080 - AG-105] SSE Transcript renderer for OpenCode session events.
 */
export class SseTranscriptConsumer {
  private sessionId: string;
  private messageCount = 0;
  private seenMessageIds = new Set<string>();
  private seenPartIds = new Set<string>();
  private seenDeltaPartIds = new Set<string>();

  public isConsuming = true;

  constructor(sessionId: string) {
    this.sessionId = sessionId;
  }

  public handleEvent(event: { type?: string; properties?: Record<string, unknown> }) {
    if (!this.isConsuming || !event?.properties) return;
    const sessionID = event.properties.sessionID as string | undefined;
    if (sessionID && sessionID !== this.sessionId) return;
    
    const eventType = event.type || '';
    if (eventType === 'session.idle') {
      this.isConsuming = false;
      return;
    }
    
    if (['server.connected', 'sync', 'session.created', 'session.updated', 'session.diff', 'session.status'].includes(eventType)) return;
    
    if (eventType === 'server.heartbeat') {
      logger.info('Processing is still ongoing...');
      return;
    }
    
    if (eventType === 'session.error') {
      logger.error({ error: event.properties }, 'Session error event received');
      this.isConsuming = false;
      return;
    }
    
    this.dispatchSpecificEvent(eventType, event.properties);
  }

  private dispatchSpecificEvent(eventType: string, properties: Record<string, unknown>) {
    if (eventType === 'message.updated' || eventType === 'message.created') {
      this.handleMessageEvent(properties);
    } else if (eventType === 'message.part.delta' || eventType === 'part.delta') {
      this.handleDeltaEvent(properties);
    } else if (eventType === 'message.part.updated' || eventType === 'part.updated') {
      this.handlePartUpdatedEvent(properties);
    }
  }

  private handleMessageEvent(properties: Record<string, unknown>) {
    const msg = (properties.message || properties) as Record<string, unknown>;
    const msgId = (msg.id || msg.messageID) as string | undefined;
    if (!msgId || this.seenMessageIds.has(msgId)) return;
    
    this.seenMessageIds.add(msgId);
    this.messageCount++;
    const role = (msg.role as string) || 'assistant';
    const userObj = msg.user as Record<string, unknown> | undefined;
    const agent = (msg.agent || userObj?.agent || '') as string;
    const modelRefObj = userObj?.modelRef as Record<string, unknown> | undefined;
    const model = (msg.model || modelRefObj?.modelId || '') as string;
    
    let header = `── msg ${this.messageCount} (role=${role}`;
    if (agent) header += ` agent=${agent}`;
    if (model) header += ` model=${model}`;
    header += `) ──`;
    printMessage(header);
  }

  private handleDeltaEvent(properties: Record<string, unknown>) {
    const partId = (properties.partID || properties.id) as string | undefined;
    if (partId && !this.seenDeltaPartIds.has(partId)) {
      this.seenDeltaPartIds.add(partId);
      printMessage('  …');
    }
  }

  private handlePartUpdatedEvent(properties: Record<string, unknown>) {
    const part = (properties.part || properties) as Record<string, unknown>;
    const partId = (part.id || part.partID) as string | undefined;
    const type = (part.type as string) || '';
    
    if (type === 'text' || type === 'reasoning') {
      if (partId && !this.seenPartIds.has(partId)) {
        this.seenPartIds.add(partId);
        const text = (part.text || part.reasoning || '') as string;
        if (text) printMessage(text);
      }
    } else {
      this.printNonTextPart(type, part);
    }
  }

  private printNonTextPart(type: string, part: Record<string, unknown>) {
    switch (type) {
      case 'tool': this.printToolPart(part); break;
      case 'file': this.printFilePart(part); break;
      case 'patch': this.printPatchPart(part); break;
      case 'step-start': printMessage('  ── step start ──'); break;
      case 'step-finish': this.printStepFinishPart(part); break;
      case 'subtask': this.printSubtaskPart(part); break;
      case 'agent': printMessage(`  👤 agent: ${part.name || part.agent}`); break;
      case 'snapshot': printMessage(`  📸 snapshot ${part.id || ''}`); break;
      case 'retry': this.printRetryPart(part); break;
      case 'compaction': printMessage(`  🗜  compaction (${part.auto ? 'auto' : 'manual'})`); break;
      default: printMessage(`  [${type}] ${JSON.stringify(part)}`); break;
    }
  }

  private printToolPart(part: Record<string, unknown>) {
    const state = (part.state as Record<string, unknown>) || {};
    const raw = (state.raw as Record<string, unknown>) || {};
    const status = (state.status || part.status || '') as string;
    let glyph = '•';
    if (status === 'completed' || status === 'done' || status === 'success') glyph = '✓';
    else if (status === 'error' || status === 'failed') glyph = '✗';
    else if (status === 'running') glyph = '⏳';
    else if (status === 'pending') glyph = '…';
    const name = (part.tool || part.name || 'tool') as string;
    const input = JSON.stringify(raw.input || part.input || '').substring(0, TRUNCATE_LENGTH_240);
    const output = JSON.stringify(raw.output || part.output || '').substring(0, TRUNCATE_LENGTH_240);
    printMessage(`  ${glyph} tool:${name} in:${input} out:${output}`);
  }

  private printFilePart(part: Record<string, unknown>) {
    const mime = (part.mime || 'file') as string;
    const filename = (part.filename || part.url || 'unnamed') as string;
    printMessage(`  📄 ${mime}: ${filename}`);
  }

  private printPatchPart(part: Record<string, unknown>) {
    const files = (part.files as string[]) || [];
    printMessage(`  🩹 patch (${files.length} files): ${files.join(', ')}`);
  }

  private printStepFinishPart(part: Record<string, unknown>) {
    const details: string[] = [];
    if (part.reason) details.push(`reason=${part.reason}`);
    if (part.cost) details.push(`cost=$${part.cost}`);
    if (part.tokens) {
      const tok = part.tokens as { input?: number; output?: number };
      details.push(`tokens=in:${tok.input || 0} out:${tok.output || 0}`);
    }
    printMessage(`  ── step finish ${details.join(' ')} ──`);
  }

  private printSubtaskPart(part: Record<string, unknown>) {
    const name = (part.agent || 'subagent') as string;
    const prompt = String(part.prompt || '').substring(0, TRUNCATE_LENGTH_160);
    printMessage(`  👥 subtask → agent=${name}: ${prompt}`);
  }

  private printRetryPart(part: Record<string, unknown>) {
    const attempt = (part.attempt || 1) as number;
    const errStr = typeof part.error === 'string' ? part.error : JSON.stringify(part.error || '');
    printMessage(`  🔁 retry attempt=${attempt}: ${errStr.substring(0, TRUNCATE_LENGTH_160)}`);
  }
}

/**
 * OpenCode Agent Prompt Execution Engine.
 */
export class OpenCodeAgentEngine {
  private client: OpencodeClient;
  private baseUrl: string;
  private activeTranscriptHandlers = new Map<string, SseTranscriptConsumer>();

  constructor(options: { baseUrl?: string; directory?: string } = {}) {
    this.baseUrl = options.baseUrl || process.env.OPENCODE_BASE_URL || 'http://localhost:4096';
    this.client = createOpencodeClient({
      baseUrl: this.baseUrl,
      directory: options.directory,
    });

    const clientWithGlobal = this.client as unknown as { global?: { event?: (cb: (ev: unknown) => void) => void } };
    if (typeof clientWithGlobal.global?.event === 'function') {
      clientWithGlobal.global.event((ev: unknown) => {
        const event = ev as { type?: string; properties?: Record<string, unknown> };
        const sessionID = event?.properties?.sessionID as string | undefined;
        if (sessionID) {
          const handler = this.activeTranscriptHandlers.get(sessionID);
          if (handler && handler.isConsuming) {
            try {
              handler.handleEvent(event);
            } catch (err: unknown) {
              if ((err as Error).name !== 'AbortError') {
                logger.warn({ err, sessionId: sessionID }, 'SSE stream raised a non-cancellation exception');
              }
            }
          }
        }
      });
    }
  }

  public getClient(): OpencodeClient {
    return this.client;
  }

  /**
   * Lists connected OpenCode providers and available models.
   */
  public async listProviders(): Promise<void> {
    await this.ensureHealthy();
    let res: { data?: ProvidersResponseData } | null = null;
    try {
      res = (await this.client.config.providers()) as { data?: ProvidersResponseData };
    } catch (err: unknown) {
      const error = err as Error;
      printError(`Failed to fetch providers: ${error.message}`);
      exitProcess(EXIT_CODE_ERROR);
    }

    const data = res?.data || {};
    const providers: ProviderInfo[] = data.providers || [];
    const connected = new Set<string>(data.connected || []);

    const connectedProviders = providers.filter((p) => connected.size === 0 || connected.has(p.id));
    const notConnectedCount = providers.length - connectedProviders.length;
    const notConnectedSuffix = notConnectedCount > 0 ? ` (${notConnectedCount} more not connected, hidden)` : '';

    printMessage(`OpenCode Providers (${connectedProviders.length} connected)${notConnectedSuffix}:`);
    for (const p of connectedProviders) {
      const modelsDict = p.models || {};
      const models = Array.isArray(modelsDict) ? modelsDict : Object.values(modelsDict);
      printMessage(`  Provider: ${p.id} (${p.name || ''})`);
      for (const m of models) {
        printMessage(`    - ${m.id} (${m.name || m.id})`);
      }
    }
  }
  public async ensureHealthy(): Promise<void> {
    try {
      const res = await this.client.config.get();
      if (!res.data) {
        throw new Error('No config response from server');
      }
    } catch (err: unknown) {
      const errorObj = err as { version?: string };
      const version = errorObj?.version || '<unknown>';
      printError(`OpenCode server at ${this.baseUrl} is not healthy (version: ${version}). Please start 'opencode serve'.`);
      exitProcess(EXIT_CODE_ERROR);
    }
  }

  /**
   * [AG-042 - AG-049] Resolves model via auto-pick or explicit spec validation.
   */
  public async resolveModel(userProvider?: string, userModel?: string): Promise<ModelRef> {
    let providersResponse: { data?: ProvidersResponseData } | null = null;
    try {
      providersResponse = (await this.client.config.providers()) as { data?: ProvidersResponseData };
    } catch {
      logger.warn(`Failed to list providers from OpenCode server at ${this.baseUrl}.`);
    }

    const providersData = providersResponse?.data || {};
    const providersList: ProviderInfo[] = providersData.providers || [];
    const connectedIds = new Set<string>(providersData.connected || []);

    if (userProvider && userModel) {
      // [AG-048 - AG-049] Explicit model verification
      if (connectedIds.size > 0 && !connectedIds.has(userProvider)) {
        printError(`Provider '${userProvider}' is not connected on OpenCode server.`);
        exitProcess(EXIT_CODE_ERROR);
      }
      return { providerId: userProvider, modelId: userModel };
    }

    // Filter connected providers
    const connectedProviders = providersList.filter((p) => connectedIds.has(p.id));

    if (connectedProviders.length === 0 && providersList.length === 0) {
      // Hardcoded fallback if inventory unavailable
      return { providerId: 'openai', modelId: 'gpt-4o-mini' };
    }

    if (connectedProviders.length === 0) {
      printError(`No connected providers available. Pass --model provider/model or configure provider on server.`);
      exitProcess(EXIT_CODE_ERROR);
    }

    // Helper to get models array
    const extractModels = (p: ProviderInfo): Array<{ id: string; name?: string }> => {
      const modelsDict = p.models || {};
      return Array.isArray(modelsDict) ? modelsDict : Object.values(modelsDict);
    };

    // [AG-046] Auto-pick logic
    // 1. Connected provider model containing 'free'
    for (const p of connectedProviders) {
      const models = extractModels(p);
      for (const m of models) {
        const idOrName = `${m.id || ''} ${m.name || ''}`.toLowerCase();
        if (idOrName.includes('free')) {
          return { providerId: p.id, modelId: m.id };
        }
      }
    }

    // 2. Any provider model containing 'free'
    for (const p of providersList) {
      const models = extractModels(p);
      for (const m of models) {
        const idOrName = `${m.id || ''} ${m.name || ''}`.toLowerCase();
        if (idOrName.includes('free')) {
          return { providerId: p.id, modelId: m.id };
        }
      }
    }

    // 3. First connected provider model
    const firstProvider = connectedProviders[0];
    const firstModels = extractModels(firstProvider);
    const firstModel = firstModels[0];
    if (firstModel) {
      return { providerId: firstProvider.id, modelId: firstModel.id };
    }

    // [AG-047] Failed auto-resolution
    printError('Unable to auto-resolve a model. Please specify --model provider/model.');
    exitProcess(EXIT_CODE_ERROR);
  }

  /**
   * [AG-050 - AG-057] Executes prompt in a new OpenCode session with live SSE transcript.
   */
  public async runPrompt(options: RunPromptOptions): Promise<string> {
    if (!options.prompt || !options.prompt.trim()) {
      throw new Error('Prompt cannot be empty or whitespace.');
    }

    await this.ensureHealthy();
    const modelRef = await this.resolveModel(options.modelProvider, options.modelName);

    const title = options.title || `DevStack.Agent @ ${new Date().toISOString()}`;
    const createRes = await this.client.session.create({
      body: { title },
    });

    const sessionData = createRes.data as { id?: string; sessionID?: string } | undefined;
    const sessionId = sessionData?.id || sessionData?.sessionID;
    if (!sessionId) {
      throw new Error('Failed to create OpenCode session.');
    }

    logger.info({ sessionId, title, model: `${modelRef.providerId}/${modelRef.modelId}` }, 'Session created');

    // Setup transcript handler for global events
    const transcriptHandler = new SseTranscriptConsumer(sessionId);
    this.activeTranscriptHandlers.set(sessionId, transcriptHandler);

    // [AG-053] Heartbeat during long LLM call
    let isHeartbeatRunning = true;
    const startTime = Date.now();
    const heartbeatTask = (async () => {
      while (isHeartbeatRunning) {
        await new Promise((resolve) => setTimeout(resolve, 30000));
        if (!isHeartbeatRunning) break;
        const elapsed = Math.floor((Date.now() - startTime) / 1000);
        logger.info(
          `still waiting... (elapsed: ${elapsed}s, session: ${sessionId}, model: ${modelRef.providerId}/${modelRef.modelId})`
        );
      }
    })();

    try {
      const promptRes = await this.client.session.prompt({
        path: { id: sessionId },
        body: {
          parts: [{ type: 'text', text: options.prompt }],
          model: {
            providerID: modelRef.providerId,
            modelID: modelRef.modelId,
          },
        },
      });

      isHeartbeatRunning = false;
      await heartbeatTask; // drain heartbeat

      // Render response summary
      const resData = promptRes.data as { info?: { role?: string; tokens?: { input?: number; output?: number }; cache?: { read?: number; write?: number }; cost?: number; finishReason?: string } } | undefined;
      const info = resData?.info;
      if (info?.role === 'assistant') {
        printMessage('\n--- Run Summary ---');
        printMessage(`model: ${modelRef.providerId}/${modelRef.modelId}`);
        if (info.tokens) printMessage(`tokens: in:${info.tokens.input || 0} out:${info.tokens.output || 0}`);
        if (info.cache) printMessage(`cache: read:${info.cache.read || 0} write:${info.cache.write || 0}`);
        if (info.cost) printMessage(`cost: $${info.cost}`);
        if (info.finishReason) printMessage(`finish: ${info.finishReason}`);
        printMessage('--------------------\n');
      }

      // [AG-085] Wait up to 3 seconds to drain closing events
      for (let i = 0; i < 30; i++) {
        if (!transcriptHandler.isConsuming) break;
        await new Promise((resolve) => setTimeout(resolve, 100));
      }
      transcriptHandler.isConsuming = false;
      this.activeTranscriptHandlers.delete(sessionId);

      printMessage(`Done. sessionId=${sessionId}`);
      return sessionId;
    } catch (err) {
      isHeartbeatRunning = false;
      await heartbeatTask;
      this.activeTranscriptHandlers.delete(sessionId);
      throw err;
    }
  }
}
