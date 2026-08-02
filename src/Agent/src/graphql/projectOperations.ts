import { DevStackGraphQLClient } from './client.js';

/**
 * [AG-180 - AG-182] Handles --list-projects execution.
 */
export async function executeListProjects(
  client: DevStackGraphQLClient,
  count = 50
): Promise<void> {
  const projects = await client.getProjects(count);

  if (projects.length === 0) {
    console.log('No projects returned by the DevStack GraphQL API.');
    process.exit(0);
  }

  console.log(`DevStack projects (${projects.length}):`);
  for (const p of projects) {
    console.log(`  id: ${p.id}`);
    console.log(`  name: ${p.name}`);
    console.log(`  repo: ${p.repository}`);
    if (p.description && p.description.trim()) {
      console.log(`  describe: ${p.description.trim()}`);
    }
    console.log('');
  }

  process.exit(0);
}

/**
 * [AG-184 - AG-186] Handles --get-project execution.
 */
export async function executeGetProject(
  client: DevStackGraphQLClient,
  uuid: string
): Promise<void> {
  const project = await client.getProjectById(uuid);

  if (!project) {
    console.log(`Project ${uuid} not found.`);
    process.exit(0);
  }

  console.log(`Project ${project.id}: ${project.name}`);
  console.log(`repo: ${project.repository}`);
  if (project.description && project.description.trim()) {
    console.log(`describe: ${project.description.trim()}`);
  }

  process.exit(0);
}
