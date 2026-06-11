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

type ApiConstructor<T> = new (configuration?: Configuration) => T

async function createAuthenticatedApi<T>(ApiClass: ApiConstructor<T>): Promise<T> {
  const token = await getAccessToken()
  if (!token) {
    throw new Error("Not authenticated")
  }
  return new ApiClass(createConfiguration(token))
}

export const createAuthenticatedUsersApi = () => createAuthenticatedApi(UsersApi)
export const createAuthenticatedSessionsApi = () => createAuthenticatedApi(SessionsApi)
export const createAuthenticatedPromptsApi = () => createAuthenticatedApi(PromptsApi)
