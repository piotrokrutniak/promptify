import { cookies } from "next/headers"

import type { AccessTokenResponse } from "@/generated/api"

export const ACCESS_TOKEN_COOKIE = "promptify.access_token"
export const REFRESH_TOKEN_COOKIE = "promptify.refresh_token"

const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 14 // 14 days

function cookieOptions(maxAge: number) {
  return {
    httpOnly: true,
    sameSite: "lax" as const,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge,
  }
}

export async function setAuthCookies(tokens: AccessTokenResponse) {
  const cookieStore = await cookies()
  const expiresIn =
    typeof tokens.expiresIn === "number" ? tokens.expiresIn : 3600

  cookieStore.set(
    ACCESS_TOKEN_COOKIE,
    tokens.accessToken,
    cookieOptions(expiresIn)
  )
  cookieStore.set(
    REFRESH_TOKEN_COOKIE,
    tokens.refreshToken,
    cookieOptions(REFRESH_TOKEN_MAX_AGE)
  )
}

export async function getAccessToken(): Promise<string | undefined> {
  const cookieStore = await cookies()
  return cookieStore.get(ACCESS_TOKEN_COOKIE)?.value
}
