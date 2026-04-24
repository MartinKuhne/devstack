import { GenericContainer, WaitUntilReadyStrategy } from 'testcontainers'

let container: GenericContainer | undefined

export async function startPostgres(): Promise<{ connectionString: string }> {
  if (container) {
    return { connectionString: container.getConnectionUri() }
  }

  container = new GenericContainer('postgres:16-alpine')
    .withExposedPorts(5432)
    .withEnvironment({
      POSTGRES_USER: 'postgres',
      POSTGRES_PASSWORD: 'postgres',
      POSTGRES_DB: 'test_devstack_graphql',
    })
    .withWaitStrategy(
      WaitUntilReadyStrategy.withLogs().withStartupTimeout(60000),
    )

  await container.start()

  const port = container.getMappedPort(5432)
  const host = container.getHost()

  return {
    connectionString: `postgresql://postgres:postgres@${host}:${port}/test_devstack_graphql`,
  }
}

export async function stopPostgres(): Promise<void> {
  if (container) {
    await container.stop()
    container = undefined
  }
}
