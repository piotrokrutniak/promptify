import { type NextRequest, NextResponse } from "next/server"

import { ACCESS_TOKEN_COOKIE } from "@/lib/auth/constants"

export function middleware(request: NextRequest) {
  const hasToken = request.cookies.has(ACCESS_TOKEN_COOKIE)
  const isSignIn = request.nextUrl.pathname === "/sign-in"

  if (!hasToken && !isSignIn) {
    return NextResponse.redirect(new URL("/sign-in", request.url))
  }

  if (hasToken && isSignIn) {
    return NextResponse.redirect(new URL("/sessions/new", request.url))
  }

  return NextResponse.next()
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
}
