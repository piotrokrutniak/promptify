import { Configuration, UsersApi } from "@/generated/api"

export function createUsersApi(accessToken?: string) {
  return new UsersApi(
    new Configuration({
      basePath: process.env.API_URL ?? "http://localhost:8080",
      accessToken,
    })
  )
}
