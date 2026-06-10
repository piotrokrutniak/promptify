import { SignInForm } from "@/features/auth"

export default function SignInPage() {
  return (
    <div className="flex min-h-svh items-center justify-center p-6">
      <div className="w-full max-w-sm">
        <SignInForm />
      </div>
    </div>
  )
}
