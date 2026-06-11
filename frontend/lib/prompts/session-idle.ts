import type { PromptDto } from "@/generated/api"

const IN_FLIGHT_STATUSES = new Set(["Pending", "Processing"])

export function isSessionIdle(prompts: PromptDto[]): boolean {
  return !prompts.some((prompt) =>
    IN_FLIGHT_STATUSES.has(prompt.status ?? "")
  )
}

export function getInFlightPromptId(prompts: PromptDto[]): number | undefined {
  for (let i = prompts.length - 1; i >= 0; i--) {
    const prompt = prompts[i]
    if (!IN_FLIGHT_STATUSES.has(prompt.status ?? "")) {
      continue
    }

    if (prompt.id !== undefined) {
      return prompt.id
    }
  }

  return undefined
}
