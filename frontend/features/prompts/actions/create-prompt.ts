"use server"

import { ResponseError } from "@/generated/api"
import { createAuthenticatedSessionsApi } from "@/lib/api-client"
import {
  createPromptSchema,
  type CreatePromptActionResult,
  type CreatePromptValues,
} from "@/lib/validations/prompt"

export async function createPromptAction(
  input: CreatePromptValues
): Promise<CreatePromptActionResult> {
  const parsed = createPromptSchema.safeParse(input)
  if (!parsed.success) {
    return {
      ok: false,
      error: parsed.error.issues[0]?.message ?? "Invalid message",
    }
  }

  try {
    const prompt = await (
      await createAuthenticatedSessionsApi()
    ).createPrompt({
      sessionId: parsed.data.sessionId,
      createPromptRequest: {
        input: parsed.data.input,
      },
    })

    return { ok: true, prompt }
  } catch (error) {
    if (error instanceof ResponseError) {
      if (error.response.status === 409) {
        return {
          ok: false,
          error: "Wait for the current prompt to finish",
        }
      }

      if (error.response.status === 400) {
        return { ok: false, error: "Invalid message" }
      }
    }

    return { ok: false, error: "Unable to send message. Is the API running?" }
  }
}
