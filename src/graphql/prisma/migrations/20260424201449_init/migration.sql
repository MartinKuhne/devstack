-- CreateEnum
CREATE TYPE "DeliverableType" AS ENUM ('FEATURE', 'DEFECT', 'MAINTENANCE');

-- CreateEnum
CREATE TYPE "DeliverableStatus" AS ENUM ('DRAFT', 'PLANNING', 'READY', 'INPROGRESS', 'DONE', 'FAILED', 'REJECTED', 'NEEDSREVIEW');

-- CreateEnum
CREATE TYPE "AgentTaskStatus" AS ENUM ('READY', 'INPROGRESS', 'DONE', 'FAILED', 'REJECTED', 'NEEDSREVIEW');

-- CreateTable
CREATE TABLE "projects" (
    "id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "name" VARCHAR(200) NOT NULL,
    "description" TEXT,
    "repository" VARCHAR(500) NOT NULL,

    CONSTRAINT "projects_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "deliverables" (
    "id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "projectId" UUID NOT NULL,
    "type" "DeliverableType" NOT NULL,
    "title" VARCHAR(200) NOT NULL,
    "status" "DeliverableStatus" NOT NULL DEFAULT 'DRAFT',
    "description" TEXT,
    "acceptanceCriteria" TEXT,
    "executionPlan" TEXT,
    "agentFeedback" TEXT,
    "securityImpact" TEXT,
    "performanceImpact" TEXT,
    "testPlan" TEXT,
    "deploymentPlan" TEXT,
    "blocking" TEXT,

    CONSTRAINT "deliverables_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "agent_tasks" (
    "id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "projectId" UUID NOT NULL,
    "deliverableId" UUID NOT NULL,
    "title" VARCHAR(300) NOT NULL,
    "status" "AgentTaskStatus" NOT NULL DEFAULT 'READY',
    "description" TEXT NOT NULL DEFAULT '',
    "result" TEXT,
    "errors" TEXT,
    "commitHash" VARCHAR(64),
    "complexityRating" INTEGER NOT NULL DEFAULT 1,
    "dependsOnAgentTaskId" UUID,
    "promptTokens" INTEGER,
    "completionTokens" INTEGER,
    "executionDurationInSeconds" INTEGER,
    "agent" VARCHAR(100),

    CONSTRAINT "agent_tasks_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "large_language_models" (
    "id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "url" VARCHAR(500) NOT NULL,
    "model" VARCHAR(200) NOT NULL,
    "modelAlias" VARCHAR(100),
    "apiKey" VARCHAR(1000) NOT NULL,
    "maxComplexity" INTEGER NOT NULL,
    "maxConcurrency" INTEGER DEFAULT 1,

    CONSTRAINT "large_language_models_pkey" PRIMARY KEY ("id")
);

-- AddForeignKey
ALTER TABLE "deliverables" ADD CONSTRAINT "deliverables_projectId_fkey" FOREIGN KEY ("projectId") REFERENCES "projects"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "agent_tasks" ADD CONSTRAINT "agent_tasks_projectId_fkey" FOREIGN KEY ("projectId") REFERENCES "projects"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "agent_tasks" ADD CONSTRAINT "agent_tasks_deliverableId_fkey" FOREIGN KEY ("deliverableId") REFERENCES "deliverables"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "agent_tasks" ADD CONSTRAINT "agent_tasks_dependsOnAgentTaskId_fkey" FOREIGN KEY ("dependsOnAgentTaskId") REFERENCES "agent_tasks"("id") ON DELETE SET NULL ON UPDATE CASCADE;
