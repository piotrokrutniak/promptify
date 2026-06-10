type PromptBubbleProps = {
  text: string
}

export function PromptBubble({ text }: PromptBubbleProps) {
  return (
    <div className="max-w-[85%] rounded-2xl rounded-br-md bg-primary px-3 py-2 text-sm whitespace-pre-wrap text-primary-foreground">
      {text}
    </div>
  )
}
