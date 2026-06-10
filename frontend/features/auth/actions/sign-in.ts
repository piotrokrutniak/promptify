"use server"

import {
  HttpValidationProblemDetailsFromJSON,
  ResponseError,
} from "@/generated/api"
import { setAuthCookies } from "@/lib/auth/cookies"
import { createUsersApi } from "@/lib/api-client"
import {
  signInSchema,
  type SignInActionResult,
  type SignInFormValues,
} from "@/lib/validations/sign-in"

function formatValidationError(body: unknown): string {
  try {
    const details = HttpValidationProblemDetailsFromJSON(body)
    const firstError = details.errors
      ? Object.values(details.errors).flat()[0]
      : undefined
    return firstError ?? details.title ?? "Invalid sign-in details"
  } catch {
    return "Invalid sign-in details"
  }
}

export async function signInAction(
  input: SignInFormValues
): Promise<SignInActionResult> {
  const parsed = signInSchema.safeParse(input)
  if (!parsed.success) {
    return {
      ok: false,
      error: parsed.error.issues[0]?.message ?? "Invalid sign-in details",
    }
  }

  try {
    const tokens = await createUsersApi().apiUsersLoginPost({
      loginRequest: {
        email: parsed.data.email,
        password: parsed.data.password,
      },
    })

    await setAuthCookies(tokens)
    return { ok: true }
  } catch (error) {
    if (error instanceof ResponseError) {
      if (error.response.status === 401) {
        return { ok: false, error: "Invalid email or password" }
      }

      if (error.response.status === 400) {
        const body = await error.response.json().catch(() => null)
        return { ok: false, error: formatValidationError(body) }
      }
    }

    return { ok: false, error: "Unable to sign in. Is the API running?" }
  }
}
