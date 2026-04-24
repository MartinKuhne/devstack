export interface PaginatedResult<T> {
  nodes: T[]
  pageInfo: PageInfo
  totalCount: number
}

export interface PageInfo {
  hasNextPage: boolean
  hasPreviousPage: boolean
  startCursor: string | null
  endCursor: string | null
}

export function cursorToOffset(cursor: string): number {
  return parseInt(cursor, 10)
}

export function offsetToCursor(offset: number): string {
  return String(offset)
}

export function paginate<T>(
  nodes: T[],
  totalCount: number,
  first?: number,
  after?: string,
): PaginatedResult<T> {
  const afterOffset = after ? cursorToOffset(after) : 0
  const limit = first ?? nodes.length

  const startOffset = afterOffset
  const slicedNodes = nodes.slice(startOffset, startOffset + limit)
  const hasNextPage = startOffset + limit < totalCount
  const hasPreviousPage = afterOffset > 0

  const startIndex = startOffset + 1
  const endIndex = Math.min(startOffset + slicedNodes.length, totalCount)

  return {
    nodes: slicedNodes,
    pageInfo: {
      hasNextPage,
      hasPreviousPage,
      startCursor: slicedNodes.length > 0 ? offsetToCursor(startIndex - 1) : null,
      endCursor: slicedNodes.length > 0 ? offsetToCursor(endIndex - 1) : null,
    },
    totalCount,
  }
}
