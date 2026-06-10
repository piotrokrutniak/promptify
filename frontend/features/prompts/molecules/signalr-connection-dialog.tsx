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
import type { SignalRConnectionStatus } from "@/lib/signalr/types"

const CONNECTING_DIALOG_DELAY_MS = 400

type SignalRConnectionDialogProps = {
  status: SignalRConnectionStatus
  onRetry: () => void
}

function getTitle(status: SignalRConnectionStatus["state"]): string {
  switch (status) {
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
  const hubLine = status.hubUrl ? `Hub: ${status.hubUrl}` : undefined

  switch (status.state) {
    case "connecting":
      return [hubLine, "Establishing a SignalR connection for prompt status updates."]
        .filter(Boolean)
        .join("\n")
    case "reconnecting":
      return [hubLine, "Connection lost. Attempting to reconnect…"]
        .filter(Boolean)
        .join("\n")
    case "disconnected":
      return [
        hubLine,
        status.error,
        status.debugDetails,
        "Live prompt status updates are unavailable.",
      ]
        .filter(Boolean)
        .join("\n\n")
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
