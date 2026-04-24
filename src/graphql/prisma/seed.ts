import { PrismaClient } from '@prisma/client'

const prisma = new PrismaClient()

async function main() {
  console.log('Seeding database...')

  const project = await prisma.project.create({
    data: {
      name: 'AI Code Generator',
      repository: 'https://github.com/example/ai-code-generator',
      description: 'An AI-powered code generation system',
    },
  })

  const deliverable1 = await prisma.deliverable.create({
    data: {
      projectId: project.id,
      title: 'Implement LLM Integration',
      type: 'FEATURE',
      description: 'Integrate large language models for code generation',
      initialStatus: 'DONE',
      acceptanceCriteria: 'LLM API calls work correctly',
    },
  })

  const deliverable2 = await prisma.deliverable.create({
    data: {
      projectId: project.id,
      title: 'Fix Authentication Bug',
      type: 'DEFECT',
      description: 'Users cannot log in with SSO',
      initialStatus: 'PLANNING',
    },
  })

  await prisma.agentTask.create({
    data: {
      projectId: project.id,
      deliverableId: deliverable1.id,
      title: 'Research LLM APIs',
      description: 'Evaluate available LLM providers',
      status: 'DONE',
      complexityRating: 5,
    },
  })

  await prisma.agentTask.create({
    data: {
      projectId: project.id,
      deliverableId: deliverable1.id,
      title: 'Implement API Client',
      description: 'Create HTTP client for LLM API',
      status: 'DONE',
      complexityRating: 7,
      dependsOnAgentTaskId: deliverable1.id,
    },
  })

  await prisma.agentTask.create({
    data: {
      projectId: project.id,
      deliverableId: deliverable1.id,
      title: 'Add Error Handling',
      description: 'Handle API errors gracefully',
      status: 'READY',
      complexityRating: 3,
    },
  })

  await prisma.agentTask.create({
    data: {
      projectId: project.id,
      deliverableId: deliverable1.id,
      title: 'Write Tests',
      description: 'Add unit and integration tests',
      status: 'READY',
      complexityRating: 4,
    },
  })

  const llm = await prisma.largeLanguageModel.create({
    data: {
      url: 'https://api.example.com/v1',
      model: 'gpt-4',
      modelAlias: 'GPT-4',
      apiKey: 'sk-test-key-12345',
      maxComplexity: 10,
      maxConcurrency: 5,
    },
  })

  console.log(`Seeded: 1 project, 2 deliverables, 4 agent tasks, 1 LLM`)
  console.log(`Project: ${project.name}`)
  console.log(`LLM: ${llm.modelAlias}`)
}

main()
  .catch((e) => {
    console.error('Seed failed:', e)
    process.exit(1)
  })
  .finally(async () => {
    await prisma.$disconnect()
  })
