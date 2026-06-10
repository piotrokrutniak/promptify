"use client"

import { useEffect, useState } from "react"

/**
 * Returns true only after `shouldOpen` stays true for `delayMs`.
 * Hides immediately when `shouldOpen` becomes false.
 */
export function useDeferredOpen(shouldOpen: boolean, delayMs: number): boolean {
  const [open, setOpen] = useState(false)

  useEffect(() => {
    if (!shouldOpen) {
      setOpen(false)
      return
    }

    const timer = window.setTimeout(() => setOpen(true), delayMs)
    return () => window.clearTimeout(timer)
  }, [shouldOpen, delayMs])

  return open
}
