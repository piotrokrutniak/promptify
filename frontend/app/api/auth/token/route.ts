import { NextResponse } from "next/server"

import {
  getAccessToken,
  getRefreshToken,
  setAuthCookies,
} from "@/lib/auth/cookies"
import { isAccessTokenExpired, refreshTokens } from "@/lib/auth/session"

export async function GET() {
  let accessToken = await getAccessToken()
  let refreshUnavailable = false

  if (!accessToken || isAccessTokenExpired(accessToken)) {
    const refreshToken = await getRefreshToken()
    if (refreshToken) {
      const result = await refreshTokens(refreshToken)
      if (result.status === "success") {
        await setAuthCookies(result.tokens)
        accessToken = result.tokens.accessToken
      } else if (result.status === "unavailable") {
        refreshUnavailable = true
      }
    }
  }

  if (refreshUnavailable) {
    return NextResponse.json({ error: "API unavailable" }, { status: 503 })
  }

  if (!accessToken) {
    return NextResponse.json({ error: "Not authenticated" }, { status: 401 })
  }

  return NextResponse.json({ accessToken })
}
