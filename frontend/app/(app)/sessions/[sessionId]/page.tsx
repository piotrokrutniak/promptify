import { notFound, redirect } from "next/navigation"

import { ResponseError } from "@/generated/api"
import { SessionChat } from "@/features/prompts"
import {
  createAuthenticatedSessionsApi,
  getPromptStatusHubUrl,
} from "@/lib/api-client"
import { requireAuth } from "@/lib/auth/require-auth"
import { parseSessionId } from "@/lib/sessions/parse-id"

type SessionPageProps = {
  params: Promise<{ sessionId: string }>
}

async function loadSession(sessionId: number) {
  try {
    return await (
      await createAuthenticatedSessionsApi()
    ).getSessionById({ sessionId })
  } catch (error) {
    if (error instanceof ResponseError && error.response.status === 404) {
      notFound()
    }

    throw error
  }
}

export default async function SessionPage({ params }: SessionPageProps) {
  const { sessionId: sessionIdParam } = await params
  const sessionId = parseSessionId(sessionIdParam)

  if (sessionId === undefined) {
    redirect("/sessions/new")
  }

  const [{ accessToken }, session] = await Promise.all([
    requireAuth(),
    loadSession(sessionId),
  ])

  return (
    <SessionChat
      mode="existing"
      sessionId={sessionId}
      initialPrompts={session.prompts ?? []}
      accessToken={accessToken}
      hubUrl={getPromptStatusHubUrl()}
    />
  )
}
