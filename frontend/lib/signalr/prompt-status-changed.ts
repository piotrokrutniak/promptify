import type { PromptDto } from "@/generated/api"
import { upsertPrompt } from "@/lib/prompts/upsert-prompt"

export type PromptStatusChangedMessage = {
  promptId: number
  sessionId: number
  orderIndex: number
  status: string
  input: string
  output: string | null
  errorMessage: string | null
}

export function applyPromptStatusChanged(
  prompts: PromptDto[],
  message: PromptStatusChangedMessage
): PromptDto[] {
  const prompt: PromptDto = {
    id: message.promptId,
    orderIndex: message.orderIndex,
    status: message.status,
    input: message.input,
    output: message.output ?? undefined,
    errorMessage: message.errorMessage ?? undefined,
  }

  return upsertPrompt(prompts, prompt)
}
