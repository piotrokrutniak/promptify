"use client"

import type { InfoResponse, SessionListItemDto } from "@/generated/api"
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar"
import { TooltipProvider } from "@/components/ui/tooltip"
import { AppSidebar } from "@/features/shell/organisms/app-sidebar"

type AppShellProps = {
  user: InfoResponse
  sessions: SessionListItemDto[]
  children: React.ReactNode
}

export function AppShell({ user, sessions, children }: AppShellProps) {
  return (
    <TooltipProvider>
      <SidebarProvider className="h-svh overflow-hidden">
        <AppSidebar user={user} sessions={sessions} />
        <SidebarInset className="h-svh min-h-0 overflow-hidden">
          <header className="flex h-12 shrink-0 items-center gap-2 border-b px-4">
            <SidebarTrigger />
          </header>
          <div className="flex min-h-0 flex-1 flex-col overflow-hidden p-6">
            {children}
          </div>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  )
}
