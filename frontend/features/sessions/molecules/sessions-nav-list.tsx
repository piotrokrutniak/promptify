import type { SessionListItemDto } from "@/generated/api"
import { SidebarMenu } from "@/components/ui/sidebar"
import { NewSessionNavLink } from "@/features/sessions/atoms/new-session-nav-link"
import { SessionNavLink } from "@/features/sessions/atoms/session-nav-link"

type SessionsNavListProps = {
  sessions: SessionListItemDto[]
}

export function SessionsNavList({ sessions }: SessionsNavListProps) {
  return (
    <SidebarMenu>
      <NewSessionNavLink />
      {sessions.length === 0 ? (
        <li className="px-2 py-1 text-sm text-muted-foreground">
          No sessions yet
        </li>
      ) : null}
      {sessions.map((session) => {
        const sessionId = session.id
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
