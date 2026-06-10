"use client"

import { useState } from "react"

import { Button } from "@/components/ui/button"

type ChatComposerProps = {
  onSubmit: (input: string) => void | Promise<void>
  disabled?: boolean
  isLoading?: boolean
}

export function ChatComposer({
  onSubmit,
  disabled = false,
  isLoading = false,
}: ChatComposerProps) {
  const [input, setInput] = useState("")

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const message = input.trim()
    if (!message || disabled || isLoading) {
      return
    }

    await onSubmit(message)
    setInput("")
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
        disabled={disabled || isLoading}
        className="w-full resize-none rounded-lg border border-input bg-transparent px-3 py-2 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:opacity-50"
      />
      <div className="flex justify-end">
        <Button type="submit" disabled={disabled || isLoading || !input.trim()}>
          {isLoading ? "Sending…" : "Send"}
        </Button>
      </div>
    </form>
  )
}
