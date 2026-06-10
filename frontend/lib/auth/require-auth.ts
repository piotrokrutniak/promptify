import { redirect } from "next/navigation"

import {
  FetchError,
  ResponseError,
  type InfoResponse,
} from "@/generated/api"
import { createAuthenticatedUsersApi } from "@/lib/api-client"
import { clearAuthCookies, getAccessToken } from "@/lib/auth/cookies"

export type AuthSession = {
  accessToken: string
  user: InfoResponse
}

export async function requireAuth(): Promise<AuthSession> {
  const accessToken = await getAccessToken()
  if (!accessToken) {
    redirect("/sign-in")
  }

  try {
    const user = await (
      await createAuthenticatedUsersApi()
    ).apiUsersManageInfoGet()
    return { accessToken, user }
  } catch (error) {
    if (
      error instanceof ResponseError &&
      (error.response.status === 401 || error.response.status === 403)
    ) {
      await clearAuthCookies()
      redirect("/sign-in")
    }

    if (error instanceof FetchError) {
      await clearAuthCookies()
      redirect("/sign-in")
    }

    throw error
  }
}
