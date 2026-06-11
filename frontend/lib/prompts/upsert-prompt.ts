import type { PromptDto } from "@/generated/api"

export function upsertPrompt(
  prompts: PromptDto[],
  prompt: PromptDto
): PromptDto[] {
  const promptId = prompt.id
  if (promptId === undefined) {
    return [...prompts, prompt]
  }

  const index = prompts.findIndex((p) => p.id === promptId)
  if (index === -1) {
    return [...prompts, prompt]
  }

  return prompts.map((p, i) => (i === index ? { ...p, ...prompt } : p))
}
