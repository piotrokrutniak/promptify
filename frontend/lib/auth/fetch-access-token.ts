export async function fetchAccessToken(): Promise<string> {
  const response = await fetch("/api/auth/token", { credentials: "include" })

  if (!response.ok) {
    throw new Error("Not authenticated")
  }

  const data: { accessToken: string } = await response.json()
  return data.accessToken
}
