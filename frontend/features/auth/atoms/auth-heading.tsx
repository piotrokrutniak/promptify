type AuthHeadingProps = {
  title: string
  subtitle?: string
}

export function AuthHeading({ title, subtitle }: AuthHeadingProps) {
  return (
    <div className="flex flex-col gap-1">
      <h1 className="text-lg font-medium">{title}</h1>
      {subtitle ? (
        <p className="text-sm text-muted-foreground">{subtitle}</p>
      ) : null}
    </div>
  )
}
