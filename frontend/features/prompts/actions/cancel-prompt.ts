"use server"

import { createAuthenticatedPromptsApi } from "@/lib/api-client"
import { runServerAction } from "@/lib/run-server-action"
import {
  cancelPromptSchema,
  type CancelPromptActionResult,
  type CancelPromptValues,
} from "@/lib/validations/prompt"

export async function cancelPromptAction(
  input: CancelPromptValues
): Promise<CancelPromptActionResult> {
  return runServerAction({
    schema: cancelPromptSchema,
    input,
    invalidFallback: "Invalid prompt",
    fallbackError: "Unable to cancel prompt. Is the API running?",
    statusErrors: {
      409: "This prompt can no longer be cancelled",
      404: "Prompt not found",
    },
    execute: async (data) => {
      await (
        await createAuthenticatedPromptsApi()
      ).cancelPrompt({ promptId: data.promptId })

      return {}
    },
  })
}
