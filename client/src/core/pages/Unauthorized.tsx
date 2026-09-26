import { Link } from "react-router-dom";
import { ROUTES } from "../../shared/routes/paths";

export function Unauthorized() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background">
      <div className="text-center">
        <h1 className="text-2xl font-semibold text-foreground">Your session has expired</h1>
        <p className="mt-2 text-muted-foreground">Log in again to keep using Jotwai.</p>
        <Link
          to={ROUTES.login}
          className="mt-4 inline-block rounded-md bg-primary px-4 py-2 font-medium text-primary-foreground hover:bg-primary/90"
        >
          Go to login
        </Link>
      </div>
    </main>
  );
}
