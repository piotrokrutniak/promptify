import { NextResponse } from "next/server"

import { getAccessToken } from "@/lib/auth/cookies"

export async function GET() {
  const accessToken = await getAccessToken()

  if (!accessToken) {
    return NextResponse.json({ error: "Not authenticated" }, { status: 401 })
  }

  return NextResponse.json({ accessToken })
}
