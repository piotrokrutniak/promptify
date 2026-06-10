const PREFIX = "promptify.chat-draft."

export function getChatDraftKey(scope: "new" | { sessionId: number }): string {
  if (scope === "new") {
    return `${PREFIX}new`
  }

  return `${PREFIX}session.${scope.sessionId}`
}

export function readChatDraft(key: string): string {
  if (typeof window === "undefined") {
    return ""
  }

  return sessionStorage.getItem(key) ?? ""
}

export function writeChatDraft(key: string, value: string): void {
  if (typeof window === "undefined") {
    return
  }

  if (value.length === 0) {
    sessionStorage.removeItem(key)
    return
  }

  sessionStorage.setItem(key, value)
}

export function clearChatDraft(key: string): void {
  if (typeof window === "undefined") {
    return
  }

  sessionStorage.removeItem(key)
}
