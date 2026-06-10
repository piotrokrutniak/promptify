import type { SessionListItemDto } from "@/generated/api"
import { SidebarMenu } from "@/components/ui/sidebar"
import { SessionNavLink } from "@/features/sessions/atoms/session-nav-link"

type SessionsNavListProps = {
  sessions: SessionListItemDto[]
}

function getSessionId(session: SessionListItemDto): number | undefined {
  const id = session.id
  if (typeof id === "number") {
    return id
  }
  return undefined
}

export function SessionsNavList({ sessions }: SessionsNavListProps) {
  if (sessions.length === 0) {
    return (
      <p className="px-2 text-sm text-muted-foreground">No sessions yet</p>
    )
  }

  return (
    <SidebarMenu>
      {sessions.map((session) => {
        const sessionId = getSessionId(session)
        if (sessionId === undefined) {
          return null
        }

        return (
          <SessionNavLink
            key={sessionId}
            sessionId={sessionId}
            title={session.title}
          />
        )
      })}
    </SidebarMenu>
  )
}
