export function shouldUseSecureCookies(): boolean {
  if (process.env.AUTH_COOKIE_SECURE === "true") return true
  if (process.env.AUTH_COOKIE_SECURE === "false") return false
  return process.env.NODE_ENV === "production"
}
