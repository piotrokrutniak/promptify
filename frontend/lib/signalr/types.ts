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
  /** Dev-facing negotiate / fetch diagnostics shown in the status dialog */
  debugDetails?: string
}
