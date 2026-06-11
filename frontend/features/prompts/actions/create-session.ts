"use server"

import { ResponseError } from "@/generated/api"
import { createAuthenticatedSessionsApi } from "@/lib/api-client"
import {
  createSessionSchema,
  type CreateSessionActionResult,
  type CreateSessionValues,
} from "@/lib/validations/prompt"

export async function createSessionAction(
  input: CreateSessionValues
): Promise<CreateSessionActionResult> {
  const parsed = createSessionSchema.safeParse(input)
  if (!parsed.success) {
    return {
      ok: false,
      error: parsed.error.issues[0]?.message ?? "Invalid message",
    }
  }

  try {
    const response = await (
      await createAuthenticatedSessionsApi()
    ).createSession({
      createSessionRequest: {
        input: parsed.data.input,
      },
    })

    const sessionId = response.sessionId
    if (sessionId === undefined) {
      return { ok: false, error: "Session created but no session id returned" }
    }

    return { ok: true, sessionId }
  } catch (error) {
    if (error instanceof ResponseError) {
      if (error.response.status === 400) {
        return { ok: false, error: "Invalid message" }
      }
    }

    return { ok: false, error: "Unable to create session. Is the API running?" }
  }
}
