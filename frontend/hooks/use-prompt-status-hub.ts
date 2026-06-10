"use client"

import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"
import { useCallback, useEffect, useRef, useState } from "react"

import {
  formatConnectionDiagnostics,
  logConnectionDiagnostics,
  probeHubNegotiate,
} from "@/lib/signalr/diagnose-hub-connection"
import type { PromptStatusChangedMessage } from "@/lib/signalr/prompt-status-changed"
import type { SignalRConnectionStatus } from "@/lib/signalr/types"

export type { SignalRConnectionState, SignalRConnectionStatus } from "@/lib/signalr/types"

type UsePromptStatusHubOptions = {
  hubUrl: string
  accessToken: string
  sessionId: number
  onStatusChanged: (message: PromptStatusChangedMessage) => void
  enabled?: boolean
}

function getErrorMessage(error: unknown): string | undefined {
  if (error instanceof Error) {
    return error.message
  }

  if (typeof error === "string") {
    return error
  }

  return undefined
}

async function reportConnectionFailure(
  hubUrl: string,
  accessToken: string,
  sessionId: number,
  connectionError: unknown
): Promise<Pick<SignalRConnectionStatus, "error" | "debugDetails">> {
  const negotiateProbe = await probeHubNegotiate(hubUrl, accessToken)
  const debugDetails = formatConnectionDiagnostics({
    hubUrl,
    sessionId,
    accessTokenPresent: accessToken.length > 0,
    accessTokenLength: accessToken.length,
    pageOrigin:
      typeof window !== "undefined" ? window.location.origin : "unknown",
    connectionError,
    negotiateProbe,
  })

  logConnectionDiagnostics(debugDetails)

  const probeSummary = negotiateProbe.ok
    ? `negotiate probe: HTTP ${negotiateProbe.status} OK (${negotiateProbe.elapsedMs}ms)`
    : negotiateProbe.fetchError
      ? `negotiate probe: ${negotiateProbe.fetchError.name ?? "Error"} — ${negotiateProbe.fetchError.message ?? "fetch failed"} (${negotiateProbe.elapsedMs}ms)`
      : `negotiate probe: HTTP ${negotiateProbe.status ?? "?"} ${negotiateProbe.statusText ?? ""} (${negotiateProbe.elapsedMs}ms)`

  const hint = negotiateProbe.hint ?? ""
  const connectionMessage = getErrorMessage(connectionError) ?? "Unknown error"

  return {
    error: [connectionMessage, probeSummary, hint].filter(Boolean).join("\n"),
    debugDetails,
  }
}

export function usePromptStatusHub({
  hubUrl,
  accessToken,
  sessionId,
  onStatusChanged,
  enabled = true,
}: UsePromptStatusHubOptions) {
  const onStatusChangedRef = useRef(onStatusChanged)
  const [retryToken, setRetryToken] = useState(0)
  const [connectionStatus, setConnectionStatus] =
    useState<SignalRConnectionStatus>({
      state: "connecting",
      hubUrl,
    })

  const retry = useCallback(() => {
    setConnectionStatus({ state: "connecting", hubUrl, debugDetails: undefined })
    setRetryToken((token) => token + 1)
  }, [hubUrl])

  const status: SignalRConnectionStatus = enabled
    ? connectionStatus
    : { state: "idle" }

  useEffect(() => {
    onStatusChangedRef.current = onStatusChanged
  }, [onStatusChanged])

  useEffect(() => {
    if (!enabled) {
      return
    }

    let cancelled = false

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    connection.on("PromptStatusChanged", (message: PromptStatusChangedMessage) => {
      onStatusChangedRef.current(message)
    })

    connection.onreconnecting(() => {
      if (!cancelled) {
        setConnectionStatus({ state: "reconnecting", hubUrl })
      }
    })

    connection.onreconnected(async () => {
      try {
        await connection.invoke("JoinSession", sessionId)
        if (!cancelled) {
          setConnectionStatus({ state: "connected", hubUrl })
        }
      } catch (error) {
        if (cancelled) {
          return
        }

        const failure = await reportConnectionFailure(
          hubUrl,
          accessToken,
          sessionId,
          error
        )
        setConnectionStatus({
          state: "disconnected",
          hubUrl,
          ...failure,
        })
      }
    })

    connection.onclose(async (error) => {
      if (cancelled) {
        return
      }

      if (error) {
        const failure = await reportConnectionFailure(
          hubUrl,
          accessToken,
          sessionId,
          error
        )
        setConnectionStatus({
          state: "disconnected",
          hubUrl,
          ...failure,
        })
        return
      }

      setConnectionStatus({
        state: "disconnected",
        hubUrl,
      })
    })

    void (async () => {
      setConnectionStatus({ state: "connecting", hubUrl, debugDetails: undefined })

      if (process.env.NODE_ENV === "development") {
        console.log("[signalr] starting connection", {
          hubUrl,
          sessionId,
          pageOrigin: window.location.origin,
          accessTokenLength: accessToken.length,
        })
      }

      try {
        await connection.start()
        if (cancelled) {
          return
        }

        await connection.invoke("JoinSession", sessionId)
        if (cancelled) {
          return
        }

        setConnectionStatus({ state: "connected", hubUrl })
      } catch (error) {
        if (cancelled) {
          if (process.env.NODE_ENV === "development") {
            console.warn(
              "[signalr] connection failed after unmount/cancel — likely aborted negotiate",
              serializeCancelledError(error)
            )
          }
          return
        }

        const failure = await reportConnectionFailure(
          hubUrl,
          accessToken,
          sessionId,
          error
        )
        setConnectionStatus({
          state: "disconnected",
          hubUrl,
          ...failure,
        })
      }
    })()

    return () => {
      cancelled = true

      if (connection.state === HubConnectionState.Connected) {
        void connection.invoke("LeaveSession", sessionId).finally(() => {
          void connection.stop()
        })
      } else {
        void connection.stop()
      }
    }
  }, [hubUrl, accessToken, sessionId, enabled, retryToken])

  return { status, retry }
}

function serializeCancelledError(error: unknown): string {
  if (error instanceof Error) {
    return `${error.name}: ${error.message}`
  }

  return String(error)
}
