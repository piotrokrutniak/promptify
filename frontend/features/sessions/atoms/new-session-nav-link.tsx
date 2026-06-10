import Link from "next/link"
import { PlusIcon } from "lucide-react"

import {
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

export function NewSessionNavLink() {
  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={<Link href="/sessions/new" />}
        tooltip="New session"
      >
        <PlusIcon />
        <span>New session</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}
