"use server"

import { redirect } from "next/navigation"

import { createAuthenticatedUsersApi } from "@/lib/api-client"
import { clearAuthCookies } from "@/lib/auth/cookies"

export async function signOutAction() {
  try {
    const usersApi = await createAuthenticatedUsersApi()
    await usersApi.logout({ body: {} })
  } catch {
    // Best-effort logout; always clear local session.
  }

  await clearAuthCookies()
  redirect("/sign-in")
}
