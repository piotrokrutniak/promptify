import { z } from "zod"

import type { PromptDto } from "@/generated/api"

export const promptInputSchema = z.object({
  input: z
    .string()
    .min(1, "Message is required")
    .max(8000, "Message must be at most 8000 characters"),
})

export const createSessionSchema = promptInputSchema

export const createPromptSchema = promptInputSchema.extend({
  sessionId: z.number().int().positive(),
})

export type PromptInputValues = z.infer<typeof promptInputSchema>
export type CreateSessionValues = z.infer<typeof createSessionSchema>
export type CreatePromptValues = z.infer<typeof createPromptSchema>

export type CreateSessionActionResult =
  | { ok: true; sessionId: number }
  | { ok: false; error: string }

export type CreatePromptActionResult =
  | { ok: true; prompt: PromptDto }
  | { ok: false; error: string }

export const cancelPromptSchema = z.object({
  promptId: z.number().int().positive(),
})

export type CancelPromptValues = z.infer<typeof cancelPromptSchema>

export type CancelPromptActionResult =
  | { ok: true }
  | { ok: false; error: string }
