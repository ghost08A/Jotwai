import { Link, Outlet } from "react-router-dom";
import { useCurrentUser } from "../../modules/identity/hooks/useCurrentUser";
import { useLogout } from "../../modules/identity/hooks/useLogout";
import { Button } from "@/shared/components/ui/button";
import { ThemeToggle } from "../../shared/components/ThemeToggle";
import { ROUTES } from "../../shared/routes/paths";

export function PrivateLayout() {
  const { data: currentUser } = useCurrentUser();
  const logout = useLogout();

  return (
    <div className="min-h-screen bg-muted dark:bg-background">
      <header className="flex items-center justify-between border-b border-border bg-card px-6 py-4 shadow-sm">
        <div className="flex items-center gap-6">
          <span className="text-lg font-semibold text-card-foreground">Jotwai</span>
          <nav className="flex gap-4 text-base font-medium text-muted-foreground">
            <Link to={ROUTES.home} className="hover:text-foreground">
              Home
            </Link>
            <Link to={ROUTES.categories} className="hover:text-foreground">
              Categories
            </Link>
            <Link to={ROUTES.trash} className="hover:text-foreground">
              Trash
            </Link>
          </nav>
        </div>

        <div className="flex items-center gap-4">
          {currentUser && (
            <span className="text-base text-muted-foreground">Signed in as {currentUser.userId}</span>
          )}
          <ThemeToggle />
          <Button type="button" variant="outline" onClick={() => logout.mutate()} disabled={logout.isPending}>
            {logout.isPending ? "Logging out…" : "Log out"}
          </Button>
        </div>
      </header>

      <Outlet />
    </div>
  );
}
