"use server"

import {
  FetchError,
  HttpValidationProblemDetailsFromJSON,
  ResponseError,
} from "@/generated/api"
import { setAuthCookies } from "@/lib/auth/cookies"
import { createUsersApi, getApiBaseUrl } from "@/lib/api-client"
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

  const apiBaseUrl = getApiBaseUrl()
  const loginUrl = `${apiBaseUrl.replace(/\/+$/, "")}/api/Users/login`
  console.log("[sign-in] API_BASE_URL:", apiBaseUrl)
  console.log("[sign-in] POST", loginUrl)

  try {
    const tokens = await createUsersApi().apiUsersLoginPost({
      loginRequest: {
        email: parsed.data.email,
        password: parsed.data.password,
      },
    })

    console.log("[sign-in] success: 200")
    await setAuthCookies(tokens)
    return { ok: true }
  } catch (error) {
    console.error("[sign-in] error type:", error?.constructor?.name ?? typeof error)

    if (error instanceof ResponseError) {
      const bodyText = await error.response.text().catch(() => "")
      console.error("[sign-in] ResponseError status:", error.response.status)
      console.error("[sign-in] ResponseError body:", bodyText.slice(0, 200))

      if (error.response.status === 401) {
        return { ok: false, error: "Invalid email or password" }
      }

      if (error.response.status === 400) {
        let body: unknown = null
        try {
          body = bodyText ? JSON.parse(bodyText) : null
        } catch {
          body = null
        }
        return { ok: false, error: formatValidationError(body) }
      }
    }

    if (error instanceof FetchError) {
      const cause = error.cause
      console.error("[sign-in] FetchError cause:", cause.message)
      if ("code" in cause && typeof cause.code === "string") {
        console.error("[sign-in] FetchError code:", cause.code)
      }
    }

    return { ok: false, error: "Unable to sign in. Is the API running?" }
  }
}
