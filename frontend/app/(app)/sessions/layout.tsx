export default function SessionsLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return <div className="-m-6 flex min-h-0 flex-1 flex-col">{children}</div>
}
