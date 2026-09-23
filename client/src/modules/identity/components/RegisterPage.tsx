import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useRegister } from "../hooks/useRegister";
import { getErrorMessage } from "../../../shared/utils/getErrorMessage";
import { ROUTES } from "../../../shared/routes/paths";

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
    <main className="flex min-h-screen items-center justify-center bg-neutral-50">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-sm space-y-4 rounded-lg border border-neutral-200 bg-white p-8 shadow-sm"
      >
        <div>
          <h1 className="text-2xl font-semibold text-neutral-900">Create an account</h1>
          <p className="mt-1 text-sm text-neutral-500">Start capturing notes with Jotwai.</p>
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
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 focus:border-neutral-500 focus:outline-none"
          />
          <span className="mt-1 block text-xs text-neutral-400">
            At least 6 characters, with an uppercase, a lowercase, a digit, and a symbol.
          </span>
        </label>

        <label className="block">
          <span className="text-sm font-medium text-neutral-700">Confirm password</span>
          <input
            type="password"
            required
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 focus:border-neutral-500 focus:outline-none"
          />
        </label>

        {mismatchError && <p className="text-sm text-red-600">Passwords do not match.</p>}
        {register.isError && (
          <p className="text-sm text-red-600">{getErrorMessage(register.error, "Failed to register")}</p>
        )}

        <button
          type="submit"
          disabled={register.isPending}
          className="w-full cursor-pointer rounded-md bg-neutral-900 px-4 py-2 font-medium text-white hover:bg-neutral-800 active:bg-neutral-950 disabled:cursor-not-allowed disabled:bg-neutral-400"
        >
          {register.isPending ? "Creating account…" : "Create account"}
        </button>

        <p className="text-center text-sm text-neutral-500">
          Already have an account?{" "}
          <Link to={ROUTES.login} className="font-medium text-neutral-900 hover:underline">
            Log in
          </Link>
        </p>
      </form>
    </main>
  );
}
