type SessionPageProps = {
  params: Promise<{ sessionId: string }>
}

export default async function SessionPage({ params }: SessionPageProps) {
  const { sessionId } = await params

  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-lg font-medium">Session {sessionId}</h1>
      <p className="text-sm text-muted-foreground">
        Prompt workspace coming soon.
      </p>
    </div>
  )
}
