/**
 * GraphQL documents and helpers the Playwright harness uses to create and remove
 * test data. Every document here must match `src/schema.graphql` exactly: the
 * server rejects unknown fields/arguments, and a rejected setup or teardown
 * mutation fails silently (rows leak into the database).
 */

/**
 * Literal marker the server's `deleteTestData` mutation looks for. The server only
 * deletes rows whose `Project.name`, `Deliverable.title`, `AgentTask.title` or
 * `LargeLanguageModel.url` contains this marker, so every fixture value must
 * include it or the row is never reclaimed.
 */
export const TEST_DATA_MARKER = '[DeleteAfterTest]';

/** A GraphQL error entry as returned in the `errors` array of a response. */
export interface GraphQLResponseError {
    message: string;
    path?: (string | number)[];
    extensions?: Record<string, unknown>;
}

/** The shape of any GraphQL response body. */
export interface GraphQLResponse<TData> {
    data?: TData | null;
    errors?: GraphQLResponseError[];
}

/**
 * Ensures a generated name, title or url carries the `[DeleteAfterTest]` marker so
 * the server-side `deleteTestData` cleanup can reclaim the row.
 * @param value the fixture value to guard
 * @returns the value with the marker prefixed when it is not already present
 */
export function withTestDataMarker(value: string): string {
    return value.includes(TEST_DATA_MARKER) ? value : `${TEST_DATA_MARKER} ${value}`;
}

/**
 * Posts a GraphQL operation over plain HTTP and returns the parsed response body.
 * Throws when the transport fails or the server answers with a non-2xx status.
 * @param graphqlUrl the GraphQL endpoint
 * @param query the GraphQL document
 * @param variables optional variable map
 * @returns the parsed `data` / `errors` payload
 */
export async function postGraphQL<TData>(
    graphqlUrl: string,
    query: string,
    variables?: Record<string, unknown>
): Promise<GraphQLResponse<TData>> {
    const response = await fetch(graphqlUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query, variables }),
    });

    if (!response.ok) {
        throw new Error(
            `GraphQL request failed with status ${response.status} ${response.statusText}`
        );
    }

    return (await response.json()) as GraphQLResponse<TData>;
}

/** Result of a `deleteTestData` round trip. */
export interface DeleteTestDataOutcome {
    /** `true` when the backend answered the mutation at all. */
    attempted: boolean;
    /** `true` when the server reported `success: true`. */
    success: boolean;
    /** Human readable message from the server, when present. */
    message?: string;
    /** GraphQL error messages, when the operation failed. */
    errors?: string[];
}

/**
 * Runs `mutation { deleteTestData { success message } }`, which removes every row
 * tagged with the `[DeleteAfterTest]` marker. Never throws: the harness must keep
 * working when the backend is not reachable.
 * @param graphqlUrl the GraphQL endpoint
 * @returns the outcome of the cleanup attempt
 */
export async function deleteTestData(graphqlUrl: string): Promise<DeleteTestDataOutcome> {
    const document = `
        mutation DeleteTestData {
            deleteTestData {
                success
                message
            }
        }
    `;

    try {
        const result = await postGraphQL<{ deleteTestData?: { success: boolean; message?: string } | null }>(
            graphqlUrl,
            document
        );

        if (result.errors && result.errors.length > 0) {
            return {
                attempted: true,
                success: false,
                errors: result.errors.map((error) => error.message),
            };
        }

        const payload = result.data?.deleteTestData;
        return {
            attempted: true,
            success: payload?.success === true,
            message: payload?.message ?? undefined,
        };
    } catch (error) {
        return {
            attempted: false,
            success: false,
            message: error instanceof Error ? error.message : String(error),
        };
    }
}

/**
 * Deletes a single entity by id. The schema signature is `delete<Entity>(id: UUID!): Boolean!`.
 * @param graphqlUrl the GraphQL endpoint
 * @param field the mutation field, e.g. `deleteProject`
 * @param id the entity id
 * @returns GraphQL error messages, or an empty array on success
 */
export async function deleteEntityById(
    graphqlUrl: string,
    field: 'deleteProject' | 'deleteDeliverable' | 'deleteAgentTask' | 'deleteLargeLanguageModel',
    id: string
): Promise<string[]> {
    const document = `mutation DeleteEntity($id: UUID!) { ${field}(id: $id) }`;
    const result = await postGraphQL<Record<string, boolean | null>>(graphqlUrl, document, {
        id,
    });

    if (result.errors && result.errors.length > 0) {
        return result.errors.map((error) => error.message);
    }

    if (result.data?.[field] === false) {
        return [`${field} reported that nothing was deleted for id ${id}`];
    }

    return [];
}
