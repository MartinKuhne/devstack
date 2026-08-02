import { DevStackGraphQLClient } from './client.js';
import { printMessage, exitProcess, EXIT_CODE_SUCCESS } from '../cli/output.js';

/**
 * [AG-180 - AG-182] Handles --list-projects execution.
 */
export async function executeListProjects(
  client: DevStackGraphQLClient,
  count = 50
): Promise<void> {
  const projects = await client.getProjects(count);

  if (projects.length === 0) {
    printMessage('No projects returned by the DevStack GraphQL API.');
    exitProcess(EXIT_CODE_SUCCESS);
  }

  printMessage(`DevStack projects (${projects.length}):`);
  for (const p of projects) {
    printMessage(`  id: ${p.id}`);
    printMessage(`  name: ${p.name}`);
    printMessage(`  repo: ${p.repository}`);
    if (p.description && p.description.trim()) {
      printMessage(`  describe: ${p.description.trim()}`);
    }
    printMessage('');
  }

  exitProcess(EXIT_CODE_SUCCESS);
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
    printMessage(`Project ${uuid} not found.`);
    exitProcess(EXIT_CODE_SUCCESS);
  }

  printMessage(`Project ${project.id}: ${project.name}`);
  printMessage(`repo: ${project.repository}`);
  if (project.description && project.description.trim()) {
    printMessage(`describe: ${project.description.trim()}`);
  }

  const deliverables = project.deliverables || [];
  printMessage(`deliverables (${deliverables.length}):`);
  if (deliverables.length === 0) {
    printMessage('  (none)');
  } else {
    for (const d of deliverables) {
      printMessage(`  - [${d.status}] ${d.title} (${d.id}) - type: ${d.type}`);
    }
  }

  exitProcess(EXIT_CODE_SUCCESS);
}
