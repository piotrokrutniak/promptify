import type { PromptDto } from "@/generated/api"
import { parseSessionId } from "@/lib/sessions/parse-id"

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
  const index = prompts.findIndex(
    (prompt) => parseSessionId(prompt.id) === message.promptId
  )

  if (index === -1) {
    return [
      ...prompts,
      {
        id: message.promptId,
        orderIndex: message.orderIndex,
        status: message.status,
        input: message.input,
        output: message.output ?? undefined,
        errorMessage: message.errorMessage ?? undefined,
      },
    ]
  }

  return prompts.map((prompt, i) =>
    i === index
      ? {
          ...prompt,
          status: message.status,
          output: message.output ?? undefined,
          errorMessage: message.errorMessage ?? undefined,
        }
      : prompt
  )
}
