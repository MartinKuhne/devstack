type WhereClause = Record<string, unknown>

export function buildWhereClause<T extends Record<string, unknown>>(
  filters: T,
  fieldMapping: Record<string, string> = {},
): WhereClause {
  const where: WhereClause = {}

  for (const [key, value] of Object.entries(filters)) {
    if (value === undefined || value === null) continue

    const dbField = fieldMapping[key] || key

    if (Array.isArray(value)) {
      where[dbField] = { in: value }
    } else {
      where[dbField] = value
    }
  }

  return where
}

export function pruneEmptyFields<T extends Record<string, unknown>>(input: T): Record<string, unknown> {
  const result: Record<string, unknown> = {}

  for (const [key, value] of Object.entries(input)) {
    if (value !== undefined && value !== null) {
      result[key] = value
    }
  }

  return result
}
