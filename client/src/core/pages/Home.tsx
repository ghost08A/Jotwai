import { useCurrentUser } from "../../modules/identity/hooks/useCurrentUser";
import { useLogout } from "../../modules/identity/hooks/useLogout";
import { Button } from "@/shared/components/ui/button";

export function Home() {
  const { data: currentUser } = useCurrentUser();
  const logout = useLogout();

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 bg-muted">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-foreground">Jotwai</h1>
        <p className="mt-2 text-muted-foreground">Capture first, organize later.</p>
        {currentUser && (
          <p className="mt-4 text-sm text-muted-foreground">Signed in as {currentUser.userId}</p>
        )}
      </div>

      <Button type="button" variant="outline" onClick={() => logout.mutate()} disabled={logout.isPending}>
        {logout.isPending ? "Logging out…" : "Log out"}
      </Button>
    </main>
  );
}
