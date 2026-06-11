"use client"

import { useEffect, useState } from "react"

/**
 * Returns true only after `shouldOpen` stays true for `delayMs`.
 * Hides immediately when `shouldOpen` becomes false.
 */
export function useDeferredOpen(shouldOpen: boolean, delayMs: number): boolean {
  const [delayedOpen, setDelayedOpen] = useState(false)
  const [prevShouldOpen, setPrevShouldOpen] = useState(shouldOpen)

  if (shouldOpen !== prevShouldOpen) {
    setPrevShouldOpen(shouldOpen)
    if (!shouldOpen) {
      setDelayedOpen(false)
    }
  }

  useEffect(() => {
    if (!shouldOpen) {
      return
    }

    const timer = window.setTimeout(() => setDelayedOpen(true), delayMs)
    return () => window.clearTimeout(timer)
  }, [shouldOpen, delayMs])

  return shouldOpen && delayedOpen
}
