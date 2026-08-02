import { logger } from '../logger.js';

export interface CliOptions {
  prompt: string;
  modelProvider?: string;
  modelName?: string;
  listProjects?: boolean;
  listProjectsCount?: number;
  getProjectUuid?: string;
  showPlan?: boolean;
  runPlan?: boolean;
  repositoryRoot?: string;
  planPrompt?: string;
  graphqlEndpoint: string;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * [AG-020 - AG-028] Parses command-line arguments into CliOptions.
 */
export function parseCliArgs(args: string[]): CliOptions {
  const options: CliOptions = {
    prompt: 'Hello',
    graphqlEndpoint: process.env.GRAPHQL_ENDPOINT || 'http://localhost:8087/graphql',
  };

  let positionalPromptSet = false;

  for (let i = 0; i < args.length; i++) {
    const arg = args[i];

    if (arg === '--list-projects') {
      options.listProjects = true;
      const next = args[i + 1];
      if (next && !next.startsWith('--') && /^\d+$/.test(next)) {
        options.listProjectsCount = parseInt(next, 10);
        i++;
      }
    } else if (arg === '--get-project') {
      const value = args[i + 1];
      if (!value || value.startsWith('--') || !UUID_REGEX.test(value)) {
        process.stderr.write(`error: --get-project requires a valid UUID, got '${value || ''}'\n`);
        process.stderr.write(`Usage: DevStack.Agent --get-project <UUID>\n`);
        process.exit(2);
      }
      options.getProjectUuid = value;
      i++;
    } else if (arg === '--show-plan') {
      options.showPlan = true;
    } else if (arg === '--run-plan') {
      options.runPlan = true;
    } else if (arg === '--model') {
      const spec = args[i + 1];
      if (spec && !spec.startsWith('--')) {
        i++;
        const slashIndex = spec.indexOf('/');
        if (slashIndex <= 0 || slashIndex === spec.length - 1) {
          logger.warn(`Warning: Invalid model spec '${spec}'. Expected format 'provider/model'. Falling back to auto-pick.`);
        } else {
          options.modelProvider = spec.substring(0, slashIndex);
          options.modelName = spec.substring(slashIndex + 1);
        }
      }
    } else if (arg === '--repositoryRoot') {
      const val = args[i + 1];
      if (val && !val.startsWith('--')) {
        options.repositoryRoot = val;
        i++;
      }
    } else if (arg === '--plan-prompt') {
      const val = args[i + 1];
      if (val && !val.startsWith('--')) {
        options.planPrompt = val;
        i++;
      }
    } else if (!arg.startsWith('--') && !positionalPromptSet) {
      options.prompt = arg;
      positionalPromptSet = true;
    }
  }

  if (!positionalPromptSet) {
    logger.info(`Using default prompt '${options.prompt}'`);
  }

  if (!options.planPrompt) {
    options.planPrompt = 'prompts/plan.prompt';
  }

  return options;
}
