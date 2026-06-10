"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { PlusIcon } from "lucide-react"

import {
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

export function NewSessionNavLink() {
  const pathname = usePathname()
  const isActive = pathname === "/sessions/new"

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={<Link href="/sessions/new" />}
        isActive={isActive}
        tooltip="New session"
      >
        <PlusIcon />
        <span>New session</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}
