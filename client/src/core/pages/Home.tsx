import { useCurrentUser } from "../../modules/identity/hooks/useCurrentUser";
import { useLogout } from "../../modules/identity/hooks/useLogout";

export function Home() {
  const { data: currentUser } = useCurrentUser();
  const logout = useLogout();

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 bg-neutral-50">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-neutral-900">Jotwai</h1>
        <p className="mt-2 text-neutral-500">Capture first, organize later.</p>
        {currentUser && (
          <p className="mt-4 text-sm text-neutral-400">Signed in as {currentUser.userId}</p>
        )}
      </div>

      <button
        type="button"
        onClick={() => logout.mutate()}
        disabled={logout.isPending}
        className="cursor-pointer rounded-md border border-neutral-300 px-4 py-2 text-sm font-medium text-neutral-700 hover:bg-neutral-100 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {logout.isPending ? "Logging out…" : "Log out"}
      </button>
    </main>
  );
}
