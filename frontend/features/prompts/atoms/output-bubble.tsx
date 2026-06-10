type OutputBubbleProps = {
  children: React.ReactNode
}

export function OutputBubble({ children }: OutputBubbleProps) {
  return (
    <div className="max-w-[85%] rounded-2xl rounded-bl-md bg-muted px-3 py-2 text-sm text-foreground">
      {children}
    </div>
  )
}
