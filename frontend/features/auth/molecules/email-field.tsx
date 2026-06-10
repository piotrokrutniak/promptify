import { Field, FieldError, FieldLabel } from "@/components/ui/field"
import { Input } from "@/components/ui/input"

type EmailFieldProps = {
  id: string
  label: string
  error?: string
  disabled?: boolean
} & Omit<React.ComponentProps<"input">, "id" | "type">

export function EmailField({
  id,
  label,
  error,
  disabled,
  ...inputProps
}: EmailFieldProps) {
  return (
    <Field data-invalid={!!error}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Input
        id={id}
        type="email"
        autoComplete="email"
        aria-invalid={!!error}
        disabled={disabled}
        {...inputProps}
      />
      <FieldError>{error}</FieldError>
    </Field>
  )
}
