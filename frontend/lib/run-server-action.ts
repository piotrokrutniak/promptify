import { z } from "zod"

import { ResponseError } from "@/generated/api"

export class ActionError extends Error {
  constructor(message: string) {
    super(message)
    this.name = "ActionError"
  }
}

type ActionFailure = { ok: false; error: string }

export async function runServerAction<TInput, TPayload extends Record<string, unknown>>(
  options: {
    schema: z.ZodType<TInput>
    input: unknown
    invalidFallback: string
    fallbackError: string
    statusErrors?: Partial<Record<number, string>>
    execute: (data: TInput) => Promise<TPayload>
  }
): Promise<({ ok: true } & TPayload) | ActionFailure> {
  const parsed = options.schema.safeParse(options.input)
  if (!parsed.success) {
    return {
      ok: false,
      error: parsed.error.issues[0]?.message ?? options.invalidFallback,
    }
  }

  try {
    const payload = await options.execute(parsed.data)
    return { ok: true, ...payload }
  } catch (error) {
    if (error instanceof ActionError) {
      return { ok: false, error: error.message }
    }

    if (error instanceof ResponseError && options.statusErrors) {
      const message = options.statusErrors[error.response.status]
      if (message) {
        return { ok: false, error: message }
      }
    }

    return { ok: false, error: options.fallbackError }
  }
}
