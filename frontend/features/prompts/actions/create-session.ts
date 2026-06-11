"use server"

import { createAuthenticatedSessionsApi } from "@/lib/api-client"
import { ActionError, runServerAction } from "@/lib/run-server-action"
import {
  createSessionSchema,
  type CreateSessionActionResult,
  type CreateSessionValues,
} from "@/lib/validations/prompt"

export async function createSessionAction(
  input: CreateSessionValues
): Promise<CreateSessionActionResult> {
  return runServerAction({
    schema: createSessionSchema,
    input,
    invalidFallback: "Invalid message",
    fallbackError: "Unable to create session. Is the API running?",
    statusErrors: { 400: "Invalid message" },
    execute: async (data) => {
      const response = await (
        await createAuthenticatedSessionsApi()
      ).createSession({
        createSessionRequest: {
          input: data.input,
        },
      })

      if (response.sessionId === undefined) {
        throw new ActionError("Session created but no session id returned")
      }

      return { sessionId: response.sessionId }
    },
  })
}
