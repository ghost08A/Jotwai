import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useRegister } from "../hooks/useRegister";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

export function RegisterPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [mismatchError, setMismatchError] = useState(false);
  const navigate = useNavigate();
  const register = useRegister();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (password !== confirmPassword) {
      setMismatchError(true);
      return;
    }
    setMismatchError(false);

    register.mutate(
      { email, password },
      { onSuccess: () => navigate(ROUTES.home, { replace: true }) },
    );
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-muted">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-sm space-y-4 rounded-lg border border-border bg-card p-8 shadow-sm"
      >
        <div>
          <h1 className="text-2xl font-semibold text-card-foreground">Create an account</h1>
          <p className="mt-1 text-base text-muted-foreground">Start capturing notes with Jotwai.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="register-email">Email</Label>
          <Input
            id="register-email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="register-password">Password</Label>
          <Input
            id="register-password"
            type="password"
            required
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <p className="text-xs text-muted-foreground">
            At least 6 characters, with an uppercase, a lowercase, a digit, and a symbol.
          </p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="register-confirm-password">Confirm password</Label>
          <Input
            id="register-confirm-password"
            type="password"
            required
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
          />
        </div>

        {mismatchError && <p className="text-base text-destructive">Passwords do not match.</p>}
        {register.isError && (
          <p className="text-base text-destructive">{getErrorMessage(register.error, "Failed to register")}</p>
        )}

        <Button type="submit" disabled={register.isPending} className="w-full">
          {register.isPending ? "Creating account…" : "Create account"}
        </Button>

        <p className="text-center text-base text-muted-foreground">
          Already have an account?{" "}
          <Link to={ROUTES.login} className="font-medium text-foreground hover:underline">
            Log in
          </Link>
        </p>
      </form>
    </main>
  );
}
