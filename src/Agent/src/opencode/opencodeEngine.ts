import { OpencodeClient, createOpencodeClient } from '@opencode-ai/sdk';
import { logger } from '../logger.js';

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

  constructor(sessionId: string) {
    this.sessionId = sessionId;
  }

  public handleEvent(event: { type?: string; properties?: Record<string, unknown> }) {
    if (!event || !event.properties) return;
    const sessionID = event.properties.sessionID as string | undefined;

    // [AG-081] Filter to current session id
    if (sessionID && sessionID !== this.sessionId) return;

    const eventType = event.type || '';

    // [AG-082] Bookkeeping events & server.heartbeat
    if (
      ['server.connected', 'sync', 'session.created', 'session.updated', 'session.diff', 'session.status'].includes(
        eventType
      )
    ) {
      return;
    }

    if (eventType === 'server.heartbeat') {
      logger.info('Processing is still ongoing...');
      return;
    }

    if (eventType === 'session.error') {
      logger.error({ error: event.properties }, 'Session error event received');
      return;
    }

    // [AG-087 - AG-089] Per-message rendering
    if (eventType === 'message.updated' || eventType === 'message.created') {
      const msg = (event.properties.message || event.properties) as Record<string, unknown>;
      const msgId = (msg.id || msg.messageID) as string | undefined;
      if (msgId && !this.seenMessageIds.has(msgId)) {
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
        console.log(header);
      }
      return;
    }

    // [AG-090 - AG-105] Per-part rendering
    if (eventType === 'message.part.delta' || eventType === 'part.delta') {
      const partId = (event.properties.partID || event.properties.id) as string | undefined;
      if (partId && !this.seenDeltaPartIds.has(partId)) {
        this.seenDeltaPartIds.add(partId);
        // [AG-092] Print single placeholder on first delta
        process.stdout.write('  …\n');
      }
      return;
    }

    if (eventType === 'message.part.updated' || eventType === 'part.updated') {
      const part = (event.properties.part || event.properties) as Record<string, unknown>;
      const partId = (part.id || part.partID) as string | undefined;
      const type = (part.type as string) || '';

      if (type === 'text' || type === 'reasoning') {
        if (partId && !this.seenPartIds.has(partId)) {
          this.seenPartIds.add(partId);
          const text = (part.text || part.reasoning || '') as string;
          if (text) console.log(text);
        }
      } else {
        this.printNonTextPart(type, part);
      }
    }
  }

  private printNonTextPart(type: string, part: Record<string, unknown>) {
    switch (type) {
      case 'tool': {
        const state = (part.state as Record<string, unknown>) || {};
        const raw = (state.raw as Record<string, unknown>) || {};
        const status = (state.status || part.status || '') as string;
        let glyph = '•';
        if (status === 'completed' || status === 'done' || status === 'success') glyph = '✓';
        else if (status === 'error' || status === 'failed') glyph = '✗';
        else if (status === 'running') glyph = '⏳';
        else if (status === 'pending') glyph = '…';

        const name = (part.tool || part.name || 'tool') as string;
        const input = JSON.stringify(raw.input || part.input || '').substring(0, 240);
        const output = JSON.stringify(raw.output || part.output || '').substring(0, 240);
        console.log(`  ${glyph} tool:${name} in:${input} out:${output}`);
        break;
      }
      case 'file': {
        const mime = (part.mime || 'file') as string;
        const filename = (part.filename || part.url || 'unnamed') as string;
        console.log(`  📄 ${mime}: ${filename}`);
        break;
      }
      case 'patch': {
        const files = (part.files as string[]) || [];
        const paths = files.join(', ');
        console.log(`  🩹 patch (${files.length} files): ${paths}`);
        break;
      }
      case 'step-start': {
        console.log('  ── step start ──');
        break;
      }
      case 'step-finish': {
        const details: string[] = [];
        if (part.reason) details.push(`reason=${part.reason}`);
        if (part.cost) details.push(`cost=$${part.cost}`);
        if (part.tokens) {
          const tok = part.tokens as { input?: number; output?: number };
          details.push(`tokens=in:${tok.input || 0} out:${tok.output || 0}`);
        }
        console.log(`  ── step finish ${details.join(' ')} ──`);
        break;
      }
      case 'subtask': {
        const name = (part.agent || 'subagent') as string;
        const prompt = String(part.prompt || '').substring(0, 160);
        console.log(`  👥 subtask → agent=${name}: ${prompt}`);
        break;
      }
      case 'agent': {
        console.log(`  👤 agent: ${part.name || part.agent}`);
        break;
      }
      case 'snapshot': {
        console.log(`  📸 snapshot ${part.id || ''}`);
        break;
      }
      case 'retry': {
        const attempt = (part.attempt || 1) as number;
        const errStr = typeof part.error === 'string' ? part.error : JSON.stringify(part.error || '');
        console.log(`  🔁 retry attempt=${attempt}: ${errStr.substring(0, 160)}`);
        break;
      }
      case 'compaction': {
        const mode = part.auto ? 'auto' : 'manual';
        console.log(`  🗜  compaction (${mode})`);
        break;
      }
      default: {
        console.log(`  [${type}] ${JSON.stringify(part)}`);
        break;
      }
    }
  }
}

/**
 * OpenCode Agent Prompt Execution Engine.
 */
export class OpenCodeAgentEngine {
  private client: OpencodeClient;
  private baseUrl: string;

  constructor(options: { baseUrl?: string; directory?: string } = {}) {
    this.baseUrl = options.baseUrl || process.env.OPENCODE_BASE_URL || 'http://localhost:4096';
    this.client = createOpencodeClient({
      baseUrl: this.baseUrl,
      directory: options.directory,
    });
  }

  public getClient(): OpencodeClient {
    return this.client;
  }

  /**
   * [AG-040 - AG-041] Health check. Exits if unhealthy.
   */
  public async ensureHealthy(): Promise<void> {
    try {
      const res = await this.client.config.get();
      if (!res.data) {
        throw new Error('No config response from server');
      }
    } catch (err: unknown) {
      const errorObj = err as { version?: string };
      const version = errorObj?.version || '<unknown>';
      process.stderr.write(
        `error: OpenCode server at ${this.baseUrl} is not healthy (version: ${version}). Please start 'opencode serve'.\n`
      );
      process.exit(1);
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
        process.stderr.write(
          `error: Provider '${userProvider}' is not connected on OpenCode server.\n`
        );
        process.exit(1);
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
      process.stderr.write(
        `hint: No connected providers available. Pass --model provider/model or configure provider on server.\n`
      );
      process.exit(1);
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
    process.stderr.write('error: Unable to auto-resolve a model. Please specify --model provider/model.\n');
    process.exit(1);
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
    const clientWithGlobal = this.client as unknown as { global?: { event?: (cb: (ev: unknown) => void) => void } };
    if (typeof clientWithGlobal.global?.event === 'function') {
      clientWithGlobal.global.event((ev: unknown) => {
        transcriptHandler.handleEvent(ev as { type?: string; properties?: Record<string, unknown> });
      });
    }

    // [AG-053] Heartbeat during long LLM call
    const startTime = Date.now();
    const heartbeatInterval = setInterval(() => {
      const elapsed = Math.floor((Date.now() - startTime) / 1000);
      logger.info(
        `still waiting... (elapsed: ${elapsed}s, session: ${sessionId}, model: ${modelRef.providerId}/${modelRef.modelId})`
      );
    }, 30000);

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

      clearInterval(heartbeatInterval);

      // Render response summary
      const resData = promptRes.data as { info?: { role?: string; tokens?: { input?: number; output?: number }; cost?: number; finishReason?: string } } | undefined;
      const info = resData?.info;
      if (info?.role === 'assistant') {
        console.log('\n--- Run Summary ---');
        console.log(`model: ${modelRef.providerId}/${modelRef.modelId}`);
        if (info.tokens) console.log(`tokens: in:${info.tokens.input || 0} out:${info.tokens.output || 0}`);
        if (info.cost) console.log(`cost: $${info.cost}`);
        if (info.finishReason) console.log(`finish: ${info.finishReason}`);
        console.log('--------------------\n');
      }

      console.log(`Done. sessionId=${sessionId}`);
      return sessionId;
    } catch (err) {
      clearInterval(heartbeatInterval);
      throw err;
    }
  }
}
