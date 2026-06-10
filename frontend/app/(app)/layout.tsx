import { createAuthenticatedSessionsApi } from "@/lib/api-client"
import { requireAuth } from "@/lib/auth/require-auth"
import { AppShell } from "@/features/shell"

export default async function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  const { user } = await requireAuth()
  const sessions = await (
    await createAuthenticatedSessionsApi()
  ).getSessions()

  return (
    <AppShell user={user} sessions={sessions}>
      {children}
    </AppShell>
  )
}
