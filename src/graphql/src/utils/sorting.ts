type OrderBy = Record<string, 'asc' | 'desc'>

export function buildOrderBy<T extends Record<string, unknown>>(
  sortInput: T,
  defaultField: string = 'id',
  defaultDirection: 'asc' | 'desc' = 'desc',
): OrderBy {
  const result: OrderBy = {}

  for (const [key, value] of Object.entries(sortInput)) {
    if (value === undefined || value === null) continue
    const direction = (value as string).toLowerCase() as 'asc' | 'desc'
    if (direction === 'asc' || direction === 'desc') {
      result[key] = direction
    }
  }

  if (Object.keys(result).length === 0) {
    result[defaultField] = defaultDirection
  }

  return result
}
