"use client"

import { useEffect, useRef } from "react"

import type { PromptDto } from "@/generated/api"
import { ChatExchange } from "@/features/prompts/molecules/chat-exchange"

type ChatMessageListProps = {
  prompts: PromptDto[]
}

export function ChatMessageList({ prompts }: ChatMessageListProps) {
  const scrollRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const el = scrollRef.current
    if (el) {
      el.scrollTop = el.scrollHeight
    }
  }, [prompts])

  if (prompts.length === 0) {
    return (
      <div className="flex min-h-0 flex-1 items-center justify-center overflow-y-auto p-6 text-sm text-muted-foreground">
        No messages yet. Send a prompt to get started.
      </div>
    )
  }

  return (
    <div
      ref={scrollRef}
      className="flex min-h-0 flex-1 flex-col overflow-y-auto px-6 py-4"
    >
      <div className="mt-auto flex flex-col gap-6">
        {prompts.map((prompt, index) => (
          <ChatExchange key={String(prompt.id ?? index)} prompt={prompt} />
        ))}
      </div>
    </div>
  )
}
