import { signOutAction } from "@/features/auth/actions/sign-out"
import { UserInfo } from "@/features/shell/atoms/user-info"
import { Button } from "@/components/ui/button"

type SidebarUserHeaderProps = {
  email: string
}

export function SidebarUserHeader({ email }: SidebarUserHeaderProps) {
  return (
    <div className="flex flex-col gap-3">
      <UserInfo email={email} />
      <form action={signOutAction}>
        <Button type="submit" variant="outline" size="sm" className="w-full">
          Sign out
        </Button>
      </form>
    </div>
  )
}
