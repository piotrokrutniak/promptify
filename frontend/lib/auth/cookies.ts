import { cookies } from "next/headers"

import type { AccessTokenResponse } from "@/generated/api"
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
} from "@/lib/auth/constants"
import { shouldUseSecureCookies } from "@/lib/auth/secure-cookies"

export { ACCESS_TOKEN_COOKIE, REFRESH_TOKEN_COOKIE }

const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 14 // 14 days

function cookieOptions(maxAge: number) {
  return {
    httpOnly: true,
    sameSite: "lax" as const,
    secure: shouldUseSecureCookies(),
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

export async function getRefreshToken(): Promise<string | undefined> {
  const cookieStore = await cookies()
  return cookieStore.get(REFRESH_TOKEN_COOKIE)?.value
}

export async function clearAuthCookies() {
  const cookieStore = await cookies()
  cookieStore.delete(ACCESS_TOKEN_COOKIE)
  cookieStore.delete(REFRESH_TOKEN_COOKIE)
}
