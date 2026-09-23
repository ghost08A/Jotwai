import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useLogin } from "../hooks/useLogin";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";

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
    <main className="flex min-h-screen items-center justify-center bg-neutral-50">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-sm space-y-4 rounded-lg border border-neutral-200 bg-white p-8 shadow-sm"
      >
        <div>
          <h1 className="text-2xl font-semibold text-neutral-900">Log in</h1>
          <p className="mt-1 text-sm text-neutral-500">Welcome back to Jotwai.</p>
        </div>

        <label className="block">
          <span className="text-sm font-medium text-neutral-700">Email</span>
          <input
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 focus:border-neutral-500 focus:outline-none"
          />
        </label>

        <label className="block">
          <span className="text-sm font-medium text-neutral-700">Password</span>
          <input
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 focus:border-neutral-500 focus:outline-none"
          />
        </label>

        {login.isError && (
          <p className="text-sm text-red-600">{getErrorMessage(login.error, "Failed to log in")}</p>
        )}

        <button
          type="submit"
          disabled={login.isPending}
          className="w-full cursor-pointer rounded-md bg-neutral-900 px-4 py-2 font-medium text-white hover:bg-neutral-800 active:bg-neutral-950 disabled:cursor-not-allowed disabled:bg-neutral-400"
        >
          {login.isPending ? "Logging in…" : "Log in"}
        </button>

        <p className="text-center text-sm text-neutral-500">
          No account?{" "}
          <Link to={ROUTES.register} className="font-medium text-neutral-900 hover:underline">
            Register
          </Link>
        </p>
      </form>
    </main>
  );
}
