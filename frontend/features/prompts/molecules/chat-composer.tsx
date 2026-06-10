"use client"

import { SquareIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { Button } from "@/components/ui/button"
import {
  clearChatDraft,
  readChatDraft,
  writeChatDraft,
} from "@/lib/prompts/chat-draft-storage"

type ChatComposerProps = {
  onSubmit: (input: string) => void | Promise<void>
  onStop?: () => void
  canStop?: boolean
  inputDisabled?: boolean
  isLoading?: boolean
  isStopping?: boolean
  draftKey?: string
  restoreDraft?: { text: string; at: number }
}

export function ChatComposer({
  onSubmit,
  onStop,
  canStop = false,
  inputDisabled = false,
  isLoading = false,
  isStopping = false,
  draftKey,
  restoreDraft,
}: ChatComposerProps) {
  const [input, setInput] = useState("")

  useEffect(() => {
    if (!draftKey) {
      return
    }

    setInput(readChatDraft(draftKey))
  }, [draftKey])

  useEffect(() => {
    if (!restoreDraft || !draftKey) {
      return
    }

    setInput(restoreDraft.text)
    writeChatDraft(draftKey, restoreDraft.text)
  }, [draftKey, restoreDraft?.at, restoreDraft?.text])

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
    setInput("")
    if (draftKey) {
      clearChatDraft(draftKey)
    }
  }

  function handleBlur() {
    if (draftKey) {
      writeChatDraft(draftKey, input)
    }
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Escape") {
      if (canStop && onStop && !isStopping) {
        event.preventDefault()
        onStop()
      }
      return
    }

    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault()
      event.currentTarget.form?.requestSubmit()
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex shrink-0 flex-col gap-2 border-t border-border p-4"
    >
      <textarea
        value={input}
        onChange={(event) => setInput(event.target.value)}
        onBlur={handleBlur}
        onKeyDown={handleKeyDown}
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
