import { Configuration, PromptsApi, SessionsApi, UsersApi } from "@/generated/api"

import { getAccessToken } from "@/lib/auth/cookies"

const DEFAULT_BASE_URL = "http://localhost:8080"

/** Server-side API calls (server actions, middleware, route handlers). */
export function getApiBaseUrl() {
  return process.env.API_BASE_URL ?? DEFAULT_BASE_URL
}

/** Browser-reachable API URL (SignalR); falls back to API_BASE_URL for local dev. */
export function getPublicApiBaseUrl() {
  return process.env.PUBLIC_API_BASE_URL ?? getApiBaseUrl()
}

/** Browser connects directly; API CORS must allow the frontend origin. */
export function getPromptStatusHubUrl() {
  return `${getPublicApiBaseUrl().replace(/\/+$/, "")}/hubs/prompts`
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

export async function createAuthenticatedPromptsApi() {
  const token = await getAccessToken()
  if (!token) {
    throw new Error("Not authenticated")
  }
  return new PromptsApi(createConfiguration(token))
}
