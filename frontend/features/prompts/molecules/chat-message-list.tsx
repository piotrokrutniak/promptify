"use client"

import { useEffect, useRef } from "react"

import type { PromptDto } from "@/generated/api"
import { ChatExchange } from "@/features/prompts/molecules/chat-exchange"
import { parseSessionId } from "@/lib/sessions/parse-id"

type ChatMessageListProps = {
  prompts: PromptDto[]
  onCancel: (promptId: number) => void
  cancellingPromptId?: number
}

export function ChatMessageList({
  prompts,
  onCancel,
  cancellingPromptId,
}: ChatMessageListProps) {
  const scrollRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const el = scrollRef.current
    if (el) {
      el.scrollTop = el.scrollHeight
    }
  }, [prompts])

  if (prompts.length === 0) {
    return (
      <div className="flex flex-1 items-center justify-center p-6 text-sm text-muted-foreground">
        No messages yet. Send a prompt to get started.
      </div>
    )
  }

  return (
    <div
      ref={scrollRef}
      className="flex min-h-0 flex-1 flex-col justify-end gap-6 overflow-y-auto px-6 py-4"
    >
      {prompts.map((prompt, index) => (
        <ChatExchange
          key={String(prompt.id ?? index)}
          prompt={prompt}
          onCancel={onCancel}
          isCancelling={cancellingPromptId === parseSessionId(prompt.id)}
        />
      ))}
    </div>
  )
}
