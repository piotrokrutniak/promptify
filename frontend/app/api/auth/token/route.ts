import { NextResponse } from "next/server"

import {
  getAccessToken,
  getRefreshToken,
  setAuthCookies,
} from "@/lib/auth/cookies"
import { isAccessTokenExpired, refreshTokens } from "@/lib/auth/session"

export async function GET() {
  let accessToken = await getAccessToken()

  if (!accessToken || isAccessTokenExpired(accessToken)) {
    const refreshToken = await getRefreshToken()
    if (refreshToken) {
      const tokens = await refreshTokens(refreshToken)
      if (tokens) {
        await setAuthCookies(tokens)
        accessToken = tokens.accessToken
      }
    }
  }

  if (!accessToken) {
    return NextResponse.json({ error: "Not authenticated" }, { status: 401 })
  }

  return NextResponse.json({ accessToken })
}
