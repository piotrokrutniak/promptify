export function DevCredentialsHint() {
  if (process.env.NODE_ENV !== "development") {
    return null
  }

  return (
    <p className="font-mono text-xs text-muted-foreground">
      Dev credentials: administrator@localhost.com / Administrator1!
    </p>
  )
}
