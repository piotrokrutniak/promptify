import { redirect } from "next/navigation"

import {
  FetchError,
  ResponseError,
  type InfoResponse,
} from "@/generated/api"
import { createAuthenticatedUsersApi } from "@/lib/api-client"
import { getAccessToken } from "@/lib/auth/cookies"

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
      redirect("/api/auth/sign-out")
    }

    if (error instanceof FetchError) {
      redirect("/api/auth/sign-out")
    }

    throw error
  }
}
