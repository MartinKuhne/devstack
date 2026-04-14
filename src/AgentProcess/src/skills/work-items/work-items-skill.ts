import { z } from 'zod';
import { GraphQLClient } from 'graphql-request';
import { ToolContext, ToolResult, ToolDefinition } from '../tool.js';

// Feature enums
const FeatureStatusEnum = z.enum([
  'DRAFT',
  'PLANNED',
  'IN_PROGRESS',
  'IN_REVIEW',
  'COMPLETED',
  'REJECTED',
]);

// Task enums
const TaskStatusEnum = z.enum([
  'TODO',
  'IN_PROGRESS',
  'REVIEW',
  'DONE',
  'BLOCKED',
]);

// Defect enums
const SeverityEnum = z.enum(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL']);

// Feature mutations
const CreateFeatureMutation = z.object({
  createFeature: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    projectId: z.string(),
    createdAt: z.string(),
  }),
});

const UpdateFeatureMutation = z.object({
  updateFeature: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    updatedAt: z.string(),
  }),
});

const TransitionFeatureStatusMutation = z.object({
  transitionFeatureStatus: z.object({
    id: z.string(),
    title: z.string(),
    status: z.string(),
  }),
});

// Task mutations
const CreateTaskMutation = z.object({
  createTask: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    complexity: z.number(),
    featureId: z.string(),
    createdAt: z.string(),
  }),
});

const UpdateTaskMutation = z.object({
  updateTask: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    complexity: z.number(),
    updatedAt: z.string(),
  }),
});

const TransitionTaskStatusMutation = z.object({
  transitionTaskStatus: z.object({
    id: z.string(),
    title: z.string(),
    status: z.string(),
  }),
});

// Defect mutations
const CreateDefectMutation = z.object({
  createDefect: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    severity: z.string().optional(),
    projectId: z.string(),
    createdAt: z.string(),
  }),
});

const UpdateDefectMutation = z.object({
  updateDefect: z.object({
    id: z.string(),
    title: z.string(),
    description: z.string().optional(),
    status: z.string(),
    updatedAt: z.string(),
  }),
});

const TransitionDefectStatusMutation = z.object({
  transitionDefectStatus: z.object({
    id: z.string(),
    title: z.string(),
    status: z.string(),
  }),
});

// Input schemas
const CreateFeatureInputSchema = z.object({
  projectId: z.string().min(1, 'Project ID is required'),
  title: z.string().min(1, 'Title is required'),
  description: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  plan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
  openQuestions: z.string().optional(),
  initialStatus: FeatureStatusEnum.optional(),
});

const UpdateFeatureInputSchema = z.object({
  id: z.string().min(1, 'Feature ID is required'),
  title: z.string().min(1).optional(),
  description: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  plan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
  openQuestions: z.string().optional(),
});

const TransitionFeatureStatusInputSchema = z.object({
  id: z.string().min(1, 'Feature ID is required'),
  targetStatus: FeatureStatusEnum,
  actor: z.string().min(1, 'Actor name is required'),
});

const CreateTaskInputSchema = z.object({
  featureId: z.string().min(1, 'Feature ID is required'),
  title: z.string().min(1, 'Title is required'),
  deliverable: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  risks: z.string().optional(),
  result: z.string().optional(),
  requiredFollowUps: z.string().optional(),
  complexityRating: z.number().int().min(1).max(10),
});

const UpdateTaskInputSchema = z.object({
  id: z.string().min(1, 'Task ID is required'),
  title: z.string().min(1).optional(),
  deliverable: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  risks: z.string().optional(),
  result: z.string().optional(),
  requiredFollowUps: z.string().optional(),
  complexityRating: z.number().int().min(1).max(10).optional(),
});

const TransitionTaskStatusInputSchema = z.object({
  id: z.string().min(1, 'Task ID is required'),
  targetStatus: TaskStatusEnum,
  actor: z.string().min(1, 'Actor name is required'),
});

const CreateDefectInputSchema = z.object({
  projectId: z.string().min(1, 'Project ID is required'),
  title: z.string().min(1, 'Title is required'),
  parentFeatureId: z.string().optional(),
  severity: SeverityEnum.optional(),
  description: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  plan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
  openQuestions: z.string().optional(),
  initialStatus: FeatureStatusEnum.optional(),
});

const UpdateDefectInputSchema = z.object({
  id: z.string().min(1, 'Defect ID is required'),
  title: z.string().min(1).optional(),
  description: z.string().optional(),
  acceptanceCriteria: z.string().optional(),
  plan: z.string().optional(),
  securityImpact: z.string().optional(),
  performanceImpact: z.string().optional(),
  testPlan: z.string().optional(),
  deploymentPlan: z.string().optional(),
  openQuestions: z.string().optional(),
});

const TransitionDefectStatusInputSchema = z.object({
  id: z.string().min(1, 'Defect ID is required'),
  targetStatus: FeatureStatusEnum,
  actor: z.string().min(1, 'Actor name is required'),
});

// Output schemas
const WorkItemOutputSchema = z.object({
  ok: z.boolean(),
  id: z.string().optional(),
  error: z.string().optional(),
});

// GraphQL mutation strings
const CREATE_FEATURE_MUTATION = `
  mutation CreateFeature($projectId: ID!, $title: String!, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String, $initialStatus: FeatureStatus) {
    createFeature(projectId: $projectId, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions, initialStatus: $initialStatus) {
      id
      title
      description
      status
      projectId
      createdAt
    }
  }
`;

const UPDATE_FEATURE_MUTATION = `
  mutation UpdateFeature($id: ID!, $title: String, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String) {
    updateFeature(id: $id, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions) {
      id
      title
      description
      status
      updatedAt
    }
  }
`;

const TRANSITION_FEATURE_STATUS_MUTATION = `
  mutation TransitionFeatureStatus($id: ID!, $targetStatus: FeatureStatus!, $actor: String!) {
    transitionFeatureStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      id
      title
      status
    }
  }
`;

const CREATE_TASK_MUTATION = `
  mutation CreateTask($featureId: ID!, $title: String!, $deliverable: String, $acceptanceCriteria: String, $risks: String, $result: String, $requiredFollowUps: String, $complexityRating: Int!) {
    createTask(featureId: $featureId, title: $title, deliverable: $deliverable, acceptanceCriteria: $acceptanceCriteria, risks: $risks, result: $result, requiredFollowUps: $requiredFollowUps, complexityRating: $complexityRating) {
      id
      title
      description
      status
      complexity
      featureId
      createdAt
    }
  }
`;

const UPDATE_TASK_MUTATION = `
  mutation UpdateTask($id: ID!, $title: String, $deliverable: String, $acceptanceCriteria: String, $risks: String, $result: String, $requiredFollowUps: String, $complexityRating: Int) {
    updateTask(id: $id, title: $title, deliverable: $deliverable, acceptanceCriteria: $acceptanceCriteria, risks: $risks, result: $result, requiredFollowUps: $requiredFollowUps, complexityRating: $complexityRating) {
      id
      title
      description
      status
      complexity
      updatedAt
    }
  }
`;

const TRANSITION_TASK_STATUS_MUTATION = `
  mutation TransitionTaskStatus($id: ID!, $targetStatus: TaskStatus!, $actor: String!) {
    transitionTaskStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      id
      title
      status
    }
  }
`;

const CREATE_DEFECT_MUTATION = `
  mutation CreateDefect($projectId: ID!, $title: String!, $parentFeatureId: ID, $severity: Severity, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String, $initialStatus: FeatureStatus) {
    createDefect(projectId: $projectId, title: $title, parentFeatureId: $parentFeatureId, severity: $severity, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions, initialStatus: $initialStatus) {
      id
      title
      description
      status
      severity
      projectId
      createdAt
    }
  }
`;

const UPDATE_DEFECT_MUTATION = `
  mutation UpdateDefect($id: ID!, $title: String, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String) {
    updateDefect(id: $id, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions) {
      id
      title
      description
      status
      updatedAt
    }
  }
`;

const TRANSITION_DEFECT_STATUS_MUTATION = `
  mutation TransitionDefectStatus($id: ID!, $targetStatus: FeatureStatus!, $actor: String!) {
    transitionDefectStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      id
      title
      status
    }
  }
`;

// Helper function to execute GraphQL mutations
async function executeMutation<T>(
  client: GraphQLClient,
  mutation: string,
  variables: Record<string, unknown>,
  context: ToolContext
): Promise<{ ok: boolean; data?: T; error?: string }> {
  try {
    context.logger.debug({ mutation: mutation.substring(0, 100), variables }, 'Executing GraphQL mutation');
    
    const result = await client.request<T>(mutation, variables);
    
    context.logger.debug('GraphQL mutation succeeded');
    
    return {
      ok: true,
      data: result,
    };
  } catch (error) {
    context.logger.error({ mutation, variables, error }, 'GraphQL mutation failed');
    
    if (error instanceof Error) {
      return {
        ok: false,
        error: error.message,
      };
    }
    
    return {
      ok: false,
      error: 'Unknown error executing mutation',
    };
  }
}

// Feature tools
export async function createFeatureTool(
  input: z.infer<typeof CreateFeatureInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ projectId: input.projectId, title: input.title }, 'Creating feature');
    
    const result = await executeMutation<{ createFeature: { id: string } }>(
      context.api,
      CREATE_FEATURE_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ featureId: result.data?.createFeature.id }, 'Feature created successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.createFeature.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to create feature');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error creating feature',
      },
    };
  }
}

export async function updateFeatureTool(
  input: z.infer<typeof UpdateFeatureInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ featureId: input.id }, 'Updating feature');
    
    const result = await executeMutation<{ updateFeature: { id: string } }>(
      context.api,
      UPDATE_FEATURE_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ featureId: result.data?.updateFeature.id }, 'Feature updated successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.updateFeature.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to update feature');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error updating feature',
      },
    };
  }
}

export async function transitionFeatureStatusTool(
  input: z.infer<typeof TransitionFeatureStatusInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ featureId: input.id, targetStatus: input.targetStatus, actor: input.actor }, 'Transitioning feature status');
    
    const result = await executeMutation<{ transitionFeatureStatus: { id: string } }>(
      context.api,
      TRANSITION_FEATURE_STATUS_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ featureId: result.data?.transitionFeatureStatus.id }, 'Feature status transitioned successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.transitionFeatureStatus.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to transition feature status');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error transitioning feature status',
      },
    };
  }
}

// Task tools
export async function createTaskTool(
  input: z.infer<typeof CreateTaskInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ featureId: input.featureId, title: input.title }, 'Creating task');
    
    const result = await executeMutation<{ createTask: { id: string } }>(
      context.api,
      CREATE_TASK_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ taskId: result.data?.createTask.id }, 'Task created successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.createTask.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to create task');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error creating task',
      },
    };
  }
}

export async function updateTaskTool(
  input: z.infer<typeof UpdateTaskInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ taskId: input.id }, 'Updating task');
    
    const result = await executeMutation<{ updateTask: { id: string } }>(
      context.api,
      UPDATE_TASK_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ taskId: result.data?.updateTask.id }, 'Task updated successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.updateTask.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to update task');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error updating task',
      },
    };
  }
}

export async function transitionTaskStatusTool(
  input: z.infer<typeof TransitionTaskStatusInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ taskId: input.id, targetStatus: input.targetStatus, actor: input.actor }, 'Transitioning task status');
    
    const result = await executeMutation<{ transitionTaskStatus: { id: string } }>(
      context.api,
      TRANSITION_TASK_STATUS_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ taskId: result.data?.transitionTaskStatus.id }, 'Task status transitioned successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.transitionTaskStatus.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to transition task status');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error transitioning task status',
      },
    };
  }
}

// Defect tools
export async function createDefectTool(
  input: z.infer<typeof CreateDefectInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ projectId: input.projectId, title: input.title }, 'Creating defect');
    
    const result = await executeMutation<{ createDefect: { id: string } }>(
      context.api,
      CREATE_DEFECT_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ defectId: result.data?.createDefect.id }, 'Defect created successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.createDefect.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to create defect');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error creating defect',
      },
    };
  }
}

export async function updateDefectTool(
  input: z.infer<typeof UpdateDefectInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ defectId: input.id }, 'Updating defect');
    
    const result = await executeMutation<{ updateDefect: { id: string } }>(
      context.api,
      UPDATE_DEFECT_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ defectId: result.data?.updateDefect.id }, 'Defect updated successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.updateDefect.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to update defect');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error updating defect',
      },
    };
  }
}

export async function transitionDefectStatusTool(
  input: z.infer<typeof TransitionDefectStatusInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WorkItemOutputSchema>>> {
  try {
    context.logger.info({ defectId: input.id, targetStatus: input.targetStatus, actor: input.actor }, 'Transitioning defect status');
    
    const result = await executeMutation<{ transitionDefectStatus: { id: string } }>(
      context.api,
      TRANSITION_DEFECT_STATUS_MUTATION,
      input,
      context
    );
    
    if (!result.ok) {
      return {
        ok: true,
        output: {
          ok: false,
          error: result.error,
        },
      };
    }
    
    context.logger.info({ defectId: result.data?.transitionDefectStatus.id }, 'Defect status transitioned successfully');
    
    return {
      ok: true,
      output: {
        ok: true,
        id: result.data?.transitionDefectStatus.id,
      },
    };
  } catch (error) {
    context.logger.error({ input, error }, 'Failed to transition defect status');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error transitioning defect status',
      },
    };
  }
}

export function createWorkItemTools(): ToolDefinition<z.ZodTypeAny, unknown>[] {
  return [
    // Feature tools
    {
      name: 'create_feature',
      description:
        'Create a new feature in a project. Requires projectId and title. Optionally provide description, acceptance criteria, and other planning fields.',
      inputSchema: CreateFeatureInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => createFeatureTool(input as z.infer<typeof CreateFeatureInputSchema>, context),
    },
    {
      name: 'update_feature',
      description:
        'Update an existing feature. Requires feature id. Provide any fields to update (title, description, acceptance criteria, etc.).',
      inputSchema: UpdateFeatureInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => updateFeatureTool(input as z.infer<typeof UpdateFeatureInputSchema>, context),
    },
    {
      name: 'transition_feature_status',
      description:
        'Transition a feature to a new status. Requires feature id, target status, and actor name. Valid statuses: DRAFT, PLANNED, IN_PROGRESS, IN_REVIEW, COMPLETED, REJECTED.',
      inputSchema: TransitionFeatureStatusInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => transitionFeatureStatusTool(input as z.infer<typeof TransitionFeatureStatusInputSchema>, context),
    },
    // Task tools
    {
      name: 'create_task',
      description:
        'Create a new task under a feature. Requires featureId, title, and complexity rating (1-10). Optionally provide deliverable, acceptance criteria, and risks.',
      inputSchema: CreateTaskInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => createTaskTool(input as z.infer<typeof CreateTaskInputSchema>, context),
    },
    {
      name: 'update_task',
      description:
        'Update an existing task. Requires task id. Provide any fields to update (title, deliverable, acceptance criteria, result, etc.).',
      inputSchema: UpdateTaskInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => updateTaskTool(input as z.infer<typeof UpdateTaskInputSchema>, context),
    },
    {
      name: 'transition_task_status',
      description:
        'Transition a task to a new status. Requires task id, target status, and actor name. Valid statuses: TODO, IN_PROGRESS, REVIEW, DONE, BLOCKED.',
      inputSchema: TransitionTaskStatusInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => transitionTaskStatusTool(input as z.infer<typeof TransitionTaskStatusInputSchema>, context),
    },
    // Defect tools
    {
      name: 'create_defect',
      description:
        'Create a new defect. Requires projectId and title. Optionally link to a parent feature, set severity, and provide description and planning fields.',
      inputSchema: CreateDefectInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => createDefectTool(input as z.infer<typeof CreateDefectInputSchema>, context),
    },
    {
      name: 'update_defect',
      description:
        'Update an existing defect. Requires defect id. Provide any fields to update (title, description, acceptance criteria, etc.).',
      inputSchema: UpdateDefectInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => updateDefectTool(input as z.infer<typeof UpdateDefectInputSchema>, context),
    },
    {
      name: 'transition_defect_status',
      description:
        'Transition a defect to a new status. Requires defect id, target status, and actor name. Valid statuses: DRAFT, PLANNED, IN_PROGRESS, IN_REVIEW, COMPLETED, REJECTED.',
      inputSchema: TransitionDefectStatusInputSchema,
      outputSchema: WorkItemOutputSchema,
      execute: async (input, context) => transitionDefectStatusTool(input as z.infer<typeof TransitionDefectStatusInputSchema>, context),
    },
  ];
}


