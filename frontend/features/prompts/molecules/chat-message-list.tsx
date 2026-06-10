import type { PromptDto } from "@/generated/api"
import { ChatMessage } from "@/features/prompts/atoms/chat-message"

type ChatMessageListProps = {
  prompts: PromptDto[]
}

export function ChatMessageList({ prompts }: ChatMessageListProps) {
  if (prompts.length === 0) {
    return (
      <div className="flex flex-1 items-center justify-center p-6 text-sm text-muted-foreground">
        No messages yet. Send a prompt to get started.
      </div>
    )
  }

  return (
    <div className="flex flex-1 flex-col overflow-y-auto px-6 py-4">
      {prompts.map((prompt, index) => (
        <ChatMessage key={String(prompt.id ?? index)} prompt={prompt} />
      ))}
    </div>
  )
}
