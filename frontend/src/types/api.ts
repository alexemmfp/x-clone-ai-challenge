export interface ProblemDetails {
  type?: string
  title: string
  status: number
  detail?: string
  errors?: Record<string, string[]>
}

export interface PagedResult<T> {
  items: T[]
  nextCursor: string | null
}
