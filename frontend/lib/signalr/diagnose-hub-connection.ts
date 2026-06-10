export type NegotiateProbeResult = {
  negotiateUrl: string
  ok: boolean
  status?: number
  statusText?: string
  bodyPreview?: string
  elapsedMs: number
  fetchError?: SerializedError
  hint?: string
}

export type SerializedError = {
  name?: string
  message?: string
  stack?: string
  cause?: SerializedError
  value?: string
}

export function serializeError(error: unknown): SerializedError {
  if (error instanceof Error) {
    return {
      name: error.name,
      message: error.message,
      stack: error.stack,
      cause:
        error.cause != null ? serializeError(error.cause) : undefined,
    }
  }

  return { value: String(error) }
}

export function getNegotiateUrl(hubUrl: string): string {
  return `${hubUrl.replace(/\/+$/, "")}/negotiate?negotiateVersion=1`
}

export async function probeHubNegotiate(
  hubUrl: string,
  accessToken: string
): Promise<NegotiateProbeResult> {
  const negotiateUrl = getNegotiateUrl(hubUrl)
  const startedAt = performance.now()

  try {
    const response = await fetch(negotiateUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: "{}",
    })

    const body = await response.text()

    return {
      negotiateUrl,
      ok: response.ok,
      status: response.status,
      statusText: response.statusText,
      bodyPreview: body.slice(0, 500),
      elapsedMs: Math.round(performance.now() - startedAt),
      hint: response.ok
        ? undefined
        : "Negotiate reached the server but returned a non-success status.",
    }
  } catch (fetchError) {
    const serialized = serializeError(fetchError)

    return {
      negotiateUrl,
      ok: false,
      elapsedMs: Math.round(performance.now() - startedAt),
      fetchError: serialized,
      hint:
        serialized.name === "TypeError" &&
        serialized.message?.includes("NetworkError")
          ? "Browser blocked or aborted the request before .NET saw it (CORS, API unreachable, mixed content, or connection torn down during negotiate)."
          : "Browser fetch to negotiate failed before a response was received.",
    }
  }
}

export function formatConnectionDiagnostics(options: {
  hubUrl: string
  sessionId: number
  accessTokenPresent: boolean
  accessTokenLength: number
  pageOrigin: string
  connectionError?: unknown
  negotiateProbe?: NegotiateProbeResult
}): string {
  const lines = [
    `pageOrigin: ${options.pageOrigin}`,
    `hubUrl: ${options.hubUrl}`,
    `sessionId: ${options.sessionId}`,
    `accessToken: ${options.accessTokenPresent ? `present (${options.accessTokenLength} chars)` : "missing"}`,
  ]

  if (options.connectionError != null) {
    lines.push(
      `connectionError: ${JSON.stringify(serializeError(options.connectionError), null, 2)}`
    )
  }

  if (options.negotiateProbe != null) {
    lines.push(
      `negotiateProbe: ${JSON.stringify(options.negotiateProbe, null, 2)}`
    )
  }

  return lines.join("\n")
}

export function logConnectionDiagnostics(diagnostics: string): void {
  console.group("[signalr] connection diagnostics")
  console.log(diagnostics)
  console.groupEnd()
}
