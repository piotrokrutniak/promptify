"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"

import {
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

type SessionNavLinkProps = {
  sessionId: number
  title?: string | null
}

export function SessionNavLink({ sessionId, title }: SessionNavLinkProps) {
  const pathname = usePathname()
  const label = title?.trim() || "Untitled session"
  const isActive = pathname === `/sessions/${sessionId}`

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={<Link href={`/sessions/${sessionId}`} />}
        isActive={isActive}
        tooltip={label}
      >
        <span className="truncate">{label}</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}
