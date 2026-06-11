import type { PromptDto } from "@/generated/api"
import { MarkdownContent } from "@/features/prompts/atoms/markdown-content"
import { OutputBubble } from "@/features/prompts/atoms/output-bubble"
import { PromptBubble } from "@/features/prompts/atoms/prompt-bubble"
import { TypingIndicator } from "@/features/prompts/atoms/typing-indicator"

type ChatExchangeProps = {
  prompt: PromptDto
}

function showTypingIndicator(prompt: PromptDto): boolean {
  const status = prompt.status ?? ""
  return (
    (status === "Pending" || status === "Processing") && !prompt.output
  )
}

export function ChatExchange({ prompt }: ChatExchangeProps) {
  const status = prompt.status ?? ""
  const showTyping = showTypingIndicator(prompt)

  return (
    <div className="flex flex-col gap-2">
      <div className="flex justify-end">
        {prompt.input ? <PromptBubble text={prompt.input} /> : null}
      </div>

      <div className="flex justify-start">
        <OutputBubble>
          {showTyping ? <TypingIndicator /> : null}
          {prompt.output ? (
            <MarkdownContent content={prompt.output} />
          ) : null}
          {prompt.errorMessage ? (
            <p className="text-destructive">{prompt.errorMessage}</p>
          ) : null}
          {status === "Cancelled" && !prompt.output && !prompt.errorMessage ? (
            <p className="text-muted-foreground">Cancelled</p>
          ) : null}
        </OutputBubble>
      </div>
    </div>
  )
}
