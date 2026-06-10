import type { NegotiateProbeResult } from "@/lib/signalr/diagnose-hub-connection"

export const SIGNALR_DISCONNECTED_PUBLIC_MESSAGE =
  "Something went wrong. Live prompt status updates are unavailable."

export function isHubUnauthorizedStatus(status?: number): boolean {
  return status === 401 || status === 403
}

export function isNegotiateUnauthorized(probe: NegotiateProbeResult): boolean {
  return isHubUnauthorizedStatus(probe.status)
}

export function connectionErrorLooksUnauthorized(error: unknown): boolean {
  const message =
    error instanceof Error
      ? error.message
      : typeof error === "string"
        ? error
        : ""

  return /Unauthorized|Status code '401'|Status code '403'|\b401\b|\b403\b/i.test(
    message
  )
}

export function shouldSignOutOnHubFailure(
  connectionError: unknown,
  negotiateProbe: NegotiateProbeResult
): boolean {
  return (
    isNegotiateUnauthorized(negotiateProbe) ||
    connectionErrorLooksUnauthorized(connectionError)
  )
}
