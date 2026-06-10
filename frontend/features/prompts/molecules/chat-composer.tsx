"use client"

import { SquareIcon } from "lucide-react"
import { useState } from "react"

import { Button } from "@/components/ui/button"

type ChatComposerProps = {
  onSubmit: (input: string) => void | Promise<void>
  onStop?: () => void
  canStop?: boolean
  inputDisabled?: boolean
  isLoading?: boolean
  isStopping?: boolean
}

export function ChatComposer({
  onSubmit,
  onStop,
  canStop = false,
  inputDisabled = false,
  isLoading = false,
  isStopping = false,
}: ChatComposerProps) {
  const [input, setInput] = useState("")

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (canStop) {
      return
    }

    const message = input.trim()
    if (!message || inputDisabled || isLoading) {
      return
    }

    await onSubmit(message)
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex shrink-0 flex-col gap-2 border-t border-border p-4"
    >
      <textarea
        value={input}
        onChange={(event) => setInput(event.target.value)}
        placeholder="Write a prompt…"
        rows={3}
        disabled={inputDisabled || isLoading || isStopping}
        className="w-full resize-none rounded-lg border border-input bg-transparent px-3 py-2 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:opacity-50"
      />
      <div className="flex justify-end">
        {canStop ? (
          <Button
            type="button"
            variant="outline"
            onClick={onStop}
            disabled={isStopping}
          >
            <SquareIcon className="fill-current" />
            {isStopping ? "Stopping…" : "Stop"}
          </Button>
        ) : (
          <Button
            type="submit"
            disabled={inputDisabled || isLoading || !input.trim()}
          >
            {isLoading ? "Sending…" : "Send"}
          </Button>
        )}
      </div>
    </form>
  )
}
