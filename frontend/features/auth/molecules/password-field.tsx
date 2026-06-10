import { Field, FieldError, FieldLabel } from "@/components/ui/field"
import { Input } from "@/components/ui/input"

type PasswordFieldProps = {
  id: string
  label: string
  error?: string
  disabled?: boolean
} & Omit<React.ComponentProps<"input">, "id" | "type">

export function PasswordField({
  id,
  label,
  error,
  disabled,
  ...inputProps
}: PasswordFieldProps) {
  return (
    <Field data-invalid={!!error}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Input
        id={id}
        type="password"
        autoComplete="current-password"
        aria-invalid={!!error}
        disabled={disabled}
        {...inputProps}
      />
      <FieldError>{error}</FieldError>
    </Field>
  )
}
