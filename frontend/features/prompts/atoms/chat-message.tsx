import type { PromptDto } from "@/generated/api"

type ChatMessageProps = {
  prompt: PromptDto
}

export function ChatMessage({ prompt }: ChatMessageProps) {
  return (
    <article className="flex flex-col gap-2 border-b border-border py-4 last:border-b-0">
      <div className="text-xs font-medium text-muted-foreground">
        Prompt {prompt.status ? `· ${prompt.status}` : ""}
      </div>
      <div className="text-sm whitespace-pre-wrap">{prompt.input}</div>
      {prompt.output ? (
        <div className="rounded-lg bg-muted px-3 py-2 text-sm whitespace-pre-wrap">
          {prompt.output}
        </div>
      ) : null}
      {prompt.errorMessage ? (
        <div className="text-sm text-destructive">{prompt.errorMessage}</div>
      ) : null}
    </article>
  )
}
