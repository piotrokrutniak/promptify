import { type NextRequest, NextResponse } from "next/server"

import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
} from "@/lib/auth/constants"
import {
  applyAuthCookiesToResponse,
  clearAuthCookiesOnResponse,
  isAccessTokenExpired,
  refreshTokens,
} from "@/lib/auth/session"

function isPublicAuthPath(pathname: string) {
  return pathname === "/sign-in" || pathname.startsWith("/api/auth/")
}

function redirectToSignIn(request: NextRequest) {
  const response = NextResponse.redirect(new URL("/sign-in", request.url))
  clearAuthCookiesOnResponse(response)
  return response
}

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl
  const isSignIn = pathname === "/sign-in"

  if (isPublicAuthPath(pathname)) {
    if (isSignIn) {
      const accessToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value
      if (accessToken && !isAccessTokenExpired(accessToken)) {
        return NextResponse.redirect(new URL("/sessions/new", request.url))
      }
    }

    return NextResponse.next()
  }

  const accessToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value
  if (accessToken && !isAccessTokenExpired(accessToken)) {
    return NextResponse.next()
  }

  const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value
  if (refreshToken) {
    const result = await refreshTokens(refreshToken)
    if (result.status === "success") {
      const response = NextResponse.next()
      applyAuthCookiesToResponse(response, result.tokens)
      return response
    }

    if (result.status === "unavailable") {
      return NextResponse.next()
    }
  }

  return redirectToSignIn(request)
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
}
