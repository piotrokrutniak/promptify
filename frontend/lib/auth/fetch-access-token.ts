import { redirectToSignOut } from "@/lib/auth/redirect-sign-out"

export async function fetchAccessToken(): Promise<string> {
  const response = await fetch("/api/auth/token", { credentials: "include" })

  if (!response.ok) {
    if (response.status === 401) {
      redirectToSignOut()
    }

    throw new Error(
      response.status === 503 ? "API unavailable" : "Not authenticated"
    )
  }

  const data: { accessToken: string } = await response.json()
  return data.accessToken
}
