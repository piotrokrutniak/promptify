"use client"

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr"
import { useEffect, useRef } from "react"

import type { PromptStatusChangedMessage } from "@/lib/signalr/prompt-status-changed"

type UsePromptStatusHubOptions = {
  hubUrl: string
  accessToken: string
  sessionId: number
  onStatusChanged: (message: PromptStatusChangedMessage) => void
  enabled?: boolean
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

    connection.onreconnected(async () => {
      try {
        await joinSession(connection, sessionId)
      } catch (error) {
        if (process.env.NODE_ENV === "development") {
          console.error("[signalr] failed to rejoin session:", error)
        }
      }
    })

    let cancelled = false

    void (async () => {
      try {
        if (process.env.NODE_ENV === "development") {
          console.log("[signalr] connecting to", hubUrl)
        }

        await connection.start()
        if (cancelled) {
          return
        }

        await joinSession(connection, sessionId)
      } catch (error) {
        if (process.env.NODE_ENV === "development") {
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
  }, [hubUrl, accessToken, sessionId, enabled])
}
