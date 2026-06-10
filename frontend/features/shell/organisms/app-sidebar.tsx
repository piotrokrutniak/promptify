import type { InfoResponse, SessionListItemDto } from "@/generated/api"
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarSeparator,
} from "@/components/ui/sidebar"
import { SessionsNavList } from "@/features/sessions"
import { SidebarUserHeader } from "@/features/shell/molecules/sidebar-user-header"

type AppSidebarProps = {
  user: InfoResponse
  sessions: SessionListItemDto[]
}

export function AppSidebar({ user, sessions }: AppSidebarProps) {
  return (
    <Sidebar>
      <SidebarHeader className="p-4">
        <SidebarUserHeader email={user.email} />
      </SidebarHeader>
      <SidebarSeparator />
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Sessions</SidebarGroupLabel>
          <SidebarGroupContent>
            <SessionsNavList sessions={sessions} />
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  )
}
