import { Configuration, UsersApi } from "@/generated/api"

const DEFAULT_BASE_URL = "http://localhost:8080"

export function getApiBaseUrl() {
  return process.env.API_BASE_URL ?? DEFAULT_BASE_URL
}

export function createUsersApi(accessToken?: string) {
  return new UsersApi(
    new Configuration({
      basePath: getApiBaseUrl(),
      accessToken,
    })
  )
}
