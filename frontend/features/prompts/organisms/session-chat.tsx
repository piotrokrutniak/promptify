"use client"

import { useState, useTransition } from "react"
import { useRouter } from "next/navigation"

import type { PromptDto } from "@/generated/api"
import { FormErrorAlert } from "@/features/auth/atoms/form-error-alert"
import { cancelPromptAction } from "@/features/prompts/actions/cancel-prompt"
import { createPromptAction } from "@/features/prompts/actions/create-prompt"
import { createSessionAction } from "@/features/prompts/actions/create-session"
import { ChatComposer } from "@/features/prompts/molecules/chat-composer"
import { ChatMessageList } from "@/features/prompts/molecules/chat-message-list"
import { isSessionIdle } from "@/lib/prompts/session-idle"
import { parseSessionId } from "@/lib/sessions/parse-id"

export type SessionChatProps =
  | { mode: "new" }
  | { mode: "existing"; sessionId: number; initialPrompts: PromptDto[] }

export function SessionChat(props: SessionChatProps) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [error, setError] = useState<string | undefined>()
  const [cancellingPromptId, setCancellingPromptId] = useState<
    number | undefined
  >()
  const [prompts, setPrompts] = useState<PromptDto[]>(
    props.mode === "existing" ? props.initialPrompts : []
  )

  const isIdle = isSessionIdle(prompts)
  const isLoading = isPending

  function handleSubmit(input: string) {
    setError(undefined)

    startTransition(async () => {
      if (props.mode === "new") {
        const result = await createSessionAction({ input })
        if (!result.ok) {
          setError(result.error)
          return
        }

        router.push(`/sessions/${result.sessionId}`)
        router.refresh()
        return
      }

      const result = await createPromptAction({
        sessionId: props.sessionId,
        input,
      })

      if (!result.ok) {
        setError(result.error)
        return
      }

      setPrompts((current) => [...current, result.prompt])
      router.refresh()
    })
  }

  function handleCancel(promptId: number) {
    setError(undefined)
    setCancellingPromptId(promptId)

    startTransition(async () => {
      const result = await cancelPromptAction({ promptId })
      setCancellingPromptId(undefined)

      if (!result.ok) {
        setError(result.error)
        return
      }

      setPrompts((current) =>
        current.map((prompt) =>
          parseSessionId(prompt.id) === promptId
            ? { ...prompt, status: "Cancelled" }
            : prompt
        )
      )
      router.refresh()
    })
  }

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <ChatMessageList
        prompts={prompts}
        onCancel={handleCancel}
        cancellingPromptId={cancellingPromptId}
      />
      {error ? (
        <div className="px-4 pb-2">
          <FormErrorAlert message={error} />
        </div>
      ) : null}
      <ChatComposer
        onSubmit={handleSubmit}
        disabled={!isIdle}
        isLoading={isLoading}
      />
    </div>
  )
}
