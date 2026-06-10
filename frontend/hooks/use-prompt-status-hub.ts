"use client"

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr"
import { useCallback, useEffect, useRef, useState } from "react"

import type { PromptStatusChangedMessage } from "@/lib/signalr/prompt-status-changed"

export type SignalRConnectionState =
  | "idle"
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected"

export type SignalRConnectionStatus = {
  state: SignalRConnectionState
  error?: string
  hubUrl?: string
}

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

async function joinSession(
  connection: HubConnection,
  sessionId: number
): Promise<void> {
  if (connection.state === HubConnectionState.Connected) {
    await connection.invoke("JoinSession", sessionId)
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
    setConnectionStatus({ state: "connecting", hubUrl })
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

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .build()

    connection.on("PromptStatusChanged", (message: PromptStatusChangedMessage) => {
      onStatusChangedRef.current(message)
    })

    let cancelled = false

    connection.onclose((error) => {
      if (cancelled) {
        return
      }

      setConnectionStatus({
        state: "disconnected",
        error: getErrorMessage(error),
        hubUrl,
      })
    })

    connection.onreconnecting(() => {
      if (cancelled) {
        return
      }

      setConnectionStatus({ state: "reconnecting", hubUrl })
    })

    connection.onreconnected(async () => {
      if (cancelled) {
        return
      }

      try {
        await joinSession(connection, sessionId)
        setConnectionStatus({ state: "connected", hubUrl })
      } catch (error) {
        if (!cancelled) {
          setConnectionStatus({
            state: "disconnected",
            error: getErrorMessage(error),
            hubUrl,
          })
        }

        if (process.env.NODE_ENV === "development") {
          console.error("[signalr] failed to rejoin session:", error)
        }
      }
    })

    void (async () => {
      setConnectionStatus({ state: "connecting", hubUrl })

      try {
        if (process.env.NODE_ENV === "development") {
          console.log("[signalr] connecting to", hubUrl)
        }

        await connection.start()
        if (cancelled) {
          return
        }

        await joinSession(connection, sessionId)
        if (cancelled) {
          return
        }

        setConnectionStatus({ state: "connected", hubUrl })
      } catch (error) {
        if (!cancelled) {
          setConnectionStatus({
            state: "disconnected",
            error: getErrorMessage(error),
            hubUrl,
          })
        }

        if (process.env.NODE_ENV === "development" && !cancelled) {
          console.error("[signalr] connection failed:", error)
        }
      }
    })()

    return () => {
      cancelled = true

      void (async () => {
        try {
          if (connection.state === HubConnectionState.Connected) {
            await connection.invoke("LeaveSession", sessionId)
          }
        } catch {
          // connection may already be closing
        }

        await connection.stop()
      })()
    }
  }, [hubUrl, accessToken, sessionId, enabled, retryToken])

  return { status, retry }
}
