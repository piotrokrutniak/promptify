"use client"

import { SquareIcon } from "lucide-react"

import { Button } from "@/components/ui/button"

type PromptStopButtonProps = {
  onClick: () => void
  disabled?: boolean
}

export function PromptStopButton({
  onClick,
  disabled = false,
}: PromptStopButtonProps) {
  return (
    <Button
      type="button"
      variant="outline"
      size="icon-sm"
      onClick={onClick}
      disabled={disabled}
      aria-label="Stop prompt"
      title="Stop"
    >
      <SquareIcon className="fill-current" />
    </Button>
  )
}
