import type { PromptDto } from "@/generated/api"

const IN_FLIGHT_STATUSES = new Set(["Pending", "Processing"])

export function isSessionIdle(prompts: PromptDto[]): boolean {
  return !prompts.some((prompt) =>
    IN_FLIGHT_STATUSES.has(prompt.status ?? "")
  )
}
