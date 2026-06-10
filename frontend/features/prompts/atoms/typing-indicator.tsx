export function TypingIndicator() {
  return (
    <div className="flex items-center gap-1 py-1" aria-label="Generating response">
      <span className="size-2 animate-pulse rounded-full bg-muted-foreground/50 [animation-delay:0ms]" />
      <span className="size-2 animate-pulse rounded-full bg-muted-foreground/50 [animation-delay:150ms]" />
      <span className="size-2 animate-pulse rounded-full bg-muted-foreground/50 [animation-delay:300ms]" />
    </div>
  )
}
