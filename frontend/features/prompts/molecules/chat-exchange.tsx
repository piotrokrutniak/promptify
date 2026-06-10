import type { PromptDto } from "@/generated/api"
import { OutputBubble } from "@/features/prompts/atoms/output-bubble"
import { PromptBubble } from "@/features/prompts/atoms/prompt-bubble"
import { PromptStopButton } from "@/features/prompts/atoms/prompt-stop-button"
import { TypingIndicator } from "@/features/prompts/atoms/typing-indicator"
import { parseSessionId } from "@/lib/sessions/parse-id"

type ChatExchangeProps = {
  prompt: PromptDto
  onCancel: (promptId: number) => void
  isCancelling?: boolean
}

function showTypingIndicator(prompt: PromptDto): boolean {
  const status = prompt.status ?? ""
  return (
    (status === "Pending" || status === "Processing") && !prompt.output
  )
}

export function ChatExchange({
  prompt,
  onCancel,
  isCancelling = false,
}: ChatExchangeProps) {
  const promptId = parseSessionId(prompt.id)
  const status = prompt.status ?? ""
  const canCancel = status === "Pending" && promptId !== undefined
  const showTyping = showTypingIndicator(prompt)

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-start justify-end gap-2">
        {prompt.input ? <PromptBubble text={prompt.input} /> : null}
        {canCancel ? (
          <PromptStopButton
            onClick={() => onCancel(promptId)}
            disabled={isCancelling}
          />
        ) : null}
      </div>

      <div className="flex justify-start">
        <OutputBubble>
          {showTyping ? <TypingIndicator /> : null}
          {prompt.output ? (
            <p className="whitespace-pre-wrap">{prompt.output}</p>
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
