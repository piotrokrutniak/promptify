import type { PromptDto } from "@/generated/api"
import { parseSessionId } from "@/lib/sessions/parse-id"

const IN_FLIGHT_STATUSES = new Set(["Pending", "Processing"])

export function isSessionIdle(prompts: PromptDto[]): boolean {
  return !prompts.some((prompt) =>
    IN_FLIGHT_STATUSES.has(prompt.status ?? "")
  )
}

export function getPendingPromptId(prompts: PromptDto[]): number | undefined {
  for (let i = prompts.length - 1; i >= 0; i--) {
    const prompt = prompts[i]
    if ((prompt.status ?? "") !== "Pending") {
      continue
    }

    const id = parseSessionId(prompt.id)
    if (id !== undefined) {
      return id
    }
  }

  return undefined
}
