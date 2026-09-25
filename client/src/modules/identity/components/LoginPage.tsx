import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useLogin } from "../hooks/useLogin";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";

export function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();
  const login = useLogin();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    login.mutate(
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
          <h1 className="text-2xl font-semibold text-card-foreground">Log in</h1>
          <p className="mt-1 text-sm text-muted-foreground">Welcome back to Jotwai.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="login-email">Email</Label>
          <Input
            id="login-email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="login-password">Password</Label>
          <Input
            id="login-password"
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {login.isError && (
          <p className="text-sm text-destructive">{getErrorMessage(login.error, "Failed to log in")}</p>
        )}

        <Button type="submit" disabled={login.isPending} className="w-full">
          {login.isPending ? "Logging in…" : "Log in"}
        </Button>

        <p className="text-center text-sm text-muted-foreground">
          No account?{" "}
          <Link to={ROUTES.register} className="font-medium text-foreground hover:underline">
            Register
          </Link>
        </p>
      </form>
    </main>
  );
}
