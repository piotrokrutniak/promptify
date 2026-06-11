import type { PromptDto } from "@/generated/api"
import { parseSessionId } from "@/lib/sessions/parse-id"

export function upsertPrompt(
  prompts: PromptDto[],
  prompt: PromptDto
): PromptDto[] {
  const promptId = parseSessionId(prompt.id)
  if (promptId === undefined) {
    return [...prompts, prompt]
  }

  const index = prompts.findIndex((p) => parseSessionId(p.id) === promptId)
  if (index === -1) {
    return [...prompts, prompt]
  }

  return prompts.map((p, i) => (i === index ? { ...p, ...prompt } : p))
}
