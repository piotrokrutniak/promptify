"use client"

import { useTransition } from "react"
import { useRouter } from "next/navigation"
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"

import { Button } from "@/components/ui/button"
import { FieldGroup } from "@/components/ui/field"
import { signInAction } from "@/features/auth/actions/sign-in"
import { AuthHeading } from "@/features/auth/atoms/auth-heading"
import { DevCredentialsHint } from "@/features/auth/atoms/dev-credentials-hint"
import { FormErrorAlert } from "@/features/auth/atoms/form-error-alert"
import { EmailField } from "@/features/auth/molecules/email-field"
import { PasswordField } from "@/features/auth/molecules/password-field"
import {
  signInSchema,
  type SignInFormValues,
} from "@/lib/validations/sign-in"

export function SignInForm() {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
    clearErrors,
  } = useForm<SignInFormValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  })

  const isLoading = isSubmitting || isPending
  const rootError = errors.root?.message

  function onSubmit(values: SignInFormValues) {
    clearErrors("root")

    startTransition(async () => {
      const result = await signInAction(values)

      if (!result.ok) {
        setError("root", { message: result.error })
        return
      }

      router.push("/sessions/new")
      router.refresh()
    })
  }

  return (
    <div className="flex w-full flex-col gap-6">
      <AuthHeading
        title="Sign in"
        subtitle="Enter your credentials to continue."
      />

      <form
        onSubmit={handleSubmit(onSubmit)}
        className="flex flex-col gap-4"
        noValidate
      >
        <FieldGroup>
          <EmailField
            id="email"
            label="Email"
            error={errors.email?.message}
            disabled={isLoading}
            {...register("email")}
          />
          <PasswordField
            id="password"
            label="Password"
            error={errors.password?.message}
            disabled={isLoading}
            {...register("password")}
          />
        </FieldGroup>

        <FormErrorAlert message={rootError} />

        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading ? "Signing in…" : "Sign in"}
        </Button>
      </form>

      <DevCredentialsHint />
    </div>
  )
}
