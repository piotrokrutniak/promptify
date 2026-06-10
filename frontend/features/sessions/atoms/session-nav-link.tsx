import Link from "next/link"

import {
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

type SessionNavLinkProps = {
  sessionId: number
  title?: string | null
}

export function SessionNavLink({ sessionId, title }: SessionNavLinkProps) {
  const label = title?.trim() || "Untitled session"

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={<Link href={`/sessions/${sessionId}`} />}
        tooltip={label}
      >
        <span className="truncate">{label}</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}
