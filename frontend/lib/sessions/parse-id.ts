export function parseRouteId(value: string): number | undefined {
  const parsed = Number.parseInt(value, 10)
  if (!Number.isNaN(parsed)) {
    return parsed
  }

  return undefined
}
