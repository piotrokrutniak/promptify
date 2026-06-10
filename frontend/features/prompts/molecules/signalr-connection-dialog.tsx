"use client"

import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { useDeferredOpen } from "@/hooks/use-deferred-open"
import { SIGNALR_DISCONNECTED_PUBLIC_MESSAGE } from "@/lib/signalr/connection-messages"
import type { SignalRConnectionStatus } from "@/lib/signalr/types"

const CONNECTING_DIALOG_DELAY_MS = 400
const isDev = process.env.NODE_ENV === "development"

type SignalRConnectionDialogProps = {
  status: SignalRConnectionStatus
  onRetry: () => void
}

function getTitle(state: SignalRConnectionStatus["state"]): string {
  switch (state) {
    case "connecting":
      return "Connecting to live updates"
    case "reconnecting":
      return "Reconnecting"
    case "disconnected":
      return "Disconnected"
    default:
      return "Live updates"
  }
}

function getDescription(status: SignalRConnectionStatus): string {
  const hubLine =
    isDev && status.hubUrl ? `Hub: ${status.hubUrl}` : undefined

  switch (status.state) {
    case "connecting":
      return [
        hubLine,
        "Establishing a connection for prompt status updates.",
      ]
        .filter(Boolean)
        .join("\n")
    case "reconnecting":
      return [hubLine, "Connection lost. Attempting to reconnect…"]
        .filter(Boolean)
        .join("\n")
    case "disconnected":
      if (isDev) {
        return [
          hubLine,
          status.error ?? SIGNALR_DISCONNECTED_PUBLIC_MESSAGE,
          status.debugDetails,
        ]
          .filter(Boolean)
          .join("\n\n")
      }

      return status.error ?? SIGNALR_DISCONNECTED_PUBLIC_MESSAGE
    default:
      return hubLine ?? ""
  }
}

export function SignalRConnectionDialog({
  status,
  onRetry,
}: SignalRConnectionDialogProps) {
  const shouldOpen =
    status.state === "connecting" ||
    status.state === "reconnecting" ||
    status.state === "disconnected"
  const openDelayMs =
    status.state === "disconnected" ? 0 : CONNECTING_DIALOG_DELAY_MS
  const open = useDeferredOpen(shouldOpen, openDelayMs)

  return (
    <Dialog open={open}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{getTitle(status.state)}</DialogTitle>
          <DialogDescription className="whitespace-pre-line">
            {getDescription(status)}
          </DialogDescription>
        </DialogHeader>
        {status.state === "disconnected" ? (
          <DialogFooter>
            <Button type="button" onClick={onRetry}>
              Retry
            </Button>
          </DialogFooter>
        ) : null}
      </DialogContent>
    </Dialog>
  )
}
