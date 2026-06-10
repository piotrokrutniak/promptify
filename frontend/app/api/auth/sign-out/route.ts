import { type NextRequest, NextResponse } from "next/server"

import { clearAuthCookiesOnResponse } from "@/lib/auth/session"

export async function GET(request: NextRequest) {
  const response = NextResponse.redirect(new URL("/sign-in", request.url))
  clearAuthCookiesOnResponse(response)
  return response
}
