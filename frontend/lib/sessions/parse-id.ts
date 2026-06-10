export function parseSessionId(value: unknown): number | undefined {
  if (typeof value === "number" && Number.isInteger(value)) {
    return value
  }

  if (typeof value === "string") {
    const parsed = Number.parseInt(value, 10)
    if (!Number.isNaN(parsed)) {
      return parsed
    }
  }

  return undefined
}
