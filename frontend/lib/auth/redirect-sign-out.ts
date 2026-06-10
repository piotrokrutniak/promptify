export function redirectToSignOut(): void {
  if (typeof window !== "undefined") {
    window.location.assign("/api/auth/sign-out")
  }
}
