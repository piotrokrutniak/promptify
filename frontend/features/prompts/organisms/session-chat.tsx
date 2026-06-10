"use client"

import { useCallback, useState, useTransition } from "react"
import { useRouter } from "next/navigation"

import type { PromptDto } from "@/generated/api"
import { FormErrorAlert } from "@/features/auth/atoms/form-error-alert"
import { cancelPromptAction } from "@/features/prompts/actions/cancel-prompt"
import { createPromptAction } from "@/features/prompts/actions/create-prompt"
import { createSessionAction } from "@/features/prompts/actions/create-session"
import { ChatComposer } from "@/features/prompts/molecules/chat-composer"
import { ChatMessageList } from "@/features/prompts/molecules/chat-message-list"
import { SignalRConnectionDialog } from "@/features/prompts/molecules/signalr-connection-dialog"
import { usePromptStatusHub } from "@/hooks/use-prompt-status-hub"
import {
  getInFlightPromptId,
  isSessionIdle,
} from "@/lib/prompts/session-idle"
import { applyPromptStatusChanged } from "@/lib/signalr/prompt-status-changed"
import { parseSessionId } from "@/lib/sessions/parse-id"

export type SessionChatProps =
  | { mode: "new" }
  | {
      mode: "existing"
      sessionId: number
      initialPrompts: PromptDto[]
      hubUrl: string
    }

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

  const handleStatusChanged = useCallback(
    (message: Parameters<typeof applyPromptStatusChanged>[1]) => {
      setPrompts((current) => applyPromptStatusChanged(current, message))
    },
    []
  )

  const { status: signalRStatus, retry: retrySignalR } = usePromptStatusHub({
    hubUrl: props.mode === "existing" ? props.hubUrl : "",
    sessionId: props.mode === "existing" ? props.sessionId : 0,
    onStatusChanged: handleStatusChanged,
    enabled: props.mode === "existing",
  })

  const isIdle = isSessionIdle(prompts)
  const inFlightPromptId = getInFlightPromptId(prompts)
  const isLoading = isPending
  const isStopping =
    inFlightPromptId !== undefined && cancellingPromptId === inFlightPromptId

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
    })
  }

  return (
    <>
      {props.mode === "existing" ? (
        <SignalRConnectionDialog
          status={signalRStatus}
          onRetry={retrySignalR}
        />
      ) : null}
      <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
        <ChatMessageList prompts={prompts} />
        {error ? (
          <div className="shrink-0 px-4 pb-2">
            <FormErrorAlert message={error} />
          </div>
        ) : null}
        <ChatComposer
          onSubmit={handleSubmit}
          onStop={
            inFlightPromptId !== undefined
              ? () => handleCancel(inFlightPromptId)
              : undefined
          }
          canStop={inFlightPromptId !== undefined}
          inputDisabled={!isIdle}
          isLoading={isLoading}
          isStopping={isStopping}
        />
      </div>
    </>
  )
}
