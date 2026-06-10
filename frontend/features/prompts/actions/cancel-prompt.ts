"use server"

import { ResponseError } from "@/generated/api"
import { createAuthenticatedPromptsApi } from "@/lib/api-client"
import {
  cancelPromptSchema,
  type CancelPromptActionResult,
  type CancelPromptValues,
} from "@/lib/validations/prompt"

export async function cancelPromptAction(
  input: CancelPromptValues
): Promise<CancelPromptActionResult> {
  const parsed = cancelPromptSchema.safeParse(input)
  if (!parsed.success) {
    return {
      ok: false,
      error: parsed.error.issues[0]?.message ?? "Invalid prompt",
    }
  }

  try {
    await (
      await createAuthenticatedPromptsApi()
    ).cancelPrompt({ promptId: parsed.data.promptId })

    return { ok: true }
  } catch (error) {
    if (error instanceof ResponseError) {
      if (error.response.status === 409) {
        return { ok: false, error: "This prompt can no longer be cancelled" }
      }

      if (error.response.status === 404) {
        return { ok: false, error: "Prompt not found" }
      }
    }

    return { ok: false, error: "Unable to cancel prompt. Is the API running?" }
  }
}
