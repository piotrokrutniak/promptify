"use server"

import { createAuthenticatedSessionsApi } from "@/lib/api-client"
import { runServerAction } from "@/lib/run-server-action"
import {
  createPromptSchema,
  type CreatePromptActionResult,
  type CreatePromptValues,
} from "@/lib/validations/prompt"

export async function createPromptAction(
  input: CreatePromptValues
): Promise<CreatePromptActionResult> {
  return runServerAction({
    schema: createPromptSchema,
    input,
    invalidFallback: "Invalid message",
    fallbackError: "Unable to send message. Is the API running?",
    statusErrors: {
      409: "Wait for the current prompt to finish",
      400: "Invalid message",
    },
    execute: async (data) => {
      const prompt = await (
        await createAuthenticatedSessionsApi()
      ).createPrompt({
        sessionId: data.sessionId,
        createPromptRequest: {
          input: data.input,
        },
      })

      return { prompt }
    },
  })
}
