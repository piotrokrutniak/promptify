import { Configuration, SessionsApi, UsersApi } from "@/generated/api"

import { getAccessToken } from "@/lib/auth/cookies"

const DEFAULT_BASE_URL = "http://localhost:8080"

export function getApiBaseUrl() {
  return process.env.API_BASE_URL ?? DEFAULT_BASE_URL
}

function createConfiguration(accessToken?: string) {
  return new Configuration({
    basePath: getApiBaseUrl(),
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  })
}

export function createUsersApi(accessToken?: string) {
  return new UsersApi(createConfiguration(accessToken))
}

export async function createAuthenticatedUsersApi() {
  const token = await getAccessToken()
  if (!token) {
    throw new Error("Not authenticated")
  }
  return new UsersApi(createConfiguration(token))
}

export async function createAuthenticatedSessionsApi() {
  const token = await getAccessToken()
  if (!token) {
    throw new Error("Not authenticated")
  }
  return new SessionsApi(createConfiguration(token))
}
