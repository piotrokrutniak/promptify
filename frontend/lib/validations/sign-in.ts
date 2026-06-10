import { z } from "zod"

export const signInSchema = z.object({
  email: z.email("Enter a valid email address"),
  password: z.string().min(1, "Password is required"),
})

export type SignInFormValues = z.infer<typeof signInSchema>

export type SignInActionResult =
  | { ok: true }
  | { ok: false; error: string }
