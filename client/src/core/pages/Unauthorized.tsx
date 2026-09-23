import { Link } from "react-router-dom";
import { ROUTES } from "../../shared/routes/paths";

export function Unauthorized() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-neutral-50">
      <div className="text-center">
        <h1 className="text-2xl font-semibold text-neutral-900">Your session has expired</h1>
        <p className="mt-2 text-neutral-500">Log in again to keep using Jotwai.</p>
        <Link
          to={ROUTES.login}
          className="mt-4 inline-block rounded-md bg-neutral-900 px-4 py-2 font-medium text-white hover:bg-neutral-800"
        >
          Go to login
        </Link>
      </div>
    </main>
  );
}
