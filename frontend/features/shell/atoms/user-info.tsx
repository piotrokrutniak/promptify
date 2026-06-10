type UserInfoProps = {
  email: string
}

export function UserInfo({ email }: UserInfoProps) {
  return (
    <div className="min-w-0">
      <p className="text-xs text-muted-foreground">Signed in as</p>
      <p className="truncate text-sm font-medium">{email}</p>
    </div>
  )
}
