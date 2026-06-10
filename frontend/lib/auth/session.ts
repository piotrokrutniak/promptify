import type { NextResponse } from "next/server"

import {
  AccessTokenResponseFromJSON,
  type AccessTokenResponse,
} from "@/generated/api"
import { getApiBaseUrl } from "@/lib/api-client"
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
} from "@/lib/auth/constants"

const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 14 // 14 days

function authCookieOptions(maxAge: number) {
  return {
    httpOnly: true,
    sameSite: "lax" as const,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge,
  }
}

export function isAccessTokenExpired(
  token: string,
  bufferSeconds = 60
): boolean {
  try {
    const payload = token.split(".")[1]
    if (!payload) {
      return true
    }

    const decoded = JSON.parse(atob(payload)) as { exp?: number }
    if (typeof decoded.exp !== "number") {
      return true
    }

    return decoded.exp * 1000 <= Date.now() + bufferSeconds * 1000
  } catch {
    return true
  }
}

export async function refreshTokens(
  refreshToken: string
): Promise<AccessTokenResponse | null> {
  const apiBaseUrl = getApiBaseUrl().replace(/\/+$/, "")
  const response = await fetch(`${apiBaseUrl}/api/Users/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  })

  if (!response.ok) {
    return null
  }

  const body: unknown = await response.json()
  return AccessTokenResponseFromJSON(body)
}

export function applyAuthCookiesToResponse(
  response: NextResponse,
  tokens: AccessTokenResponse
) {
  const expiresIn =
    typeof tokens.expiresIn === "number" ? tokens.expiresIn : 3600

  response.cookies.set(
    ACCESS_TOKEN_COOKIE,
    tokens.accessToken,
    authCookieOptions(expiresIn)
  )
  response.cookies.set(
    REFRESH_TOKEN_COOKIE,
    tokens.refreshToken,
    authCookieOptions(REFRESH_TOKEN_MAX_AGE)
  )
}

export function clearAuthCookiesOnResponse(response: NextResponse) {
  response.cookies.delete(ACCESS_TOKEN_COOKIE)
  response.cookies.delete(REFRESH_TOKEN_COOKIE)
}
