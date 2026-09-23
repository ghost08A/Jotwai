import { createContext, useContext, useSyncExternalStore } from "react";
import type { ReactNode } from "react";
import { getSession, setSession, subscribeToSession } from "../../core/http/session";
import type { Session } from "../../core/http/session";

interface AuthContextValue {
  session: Session | null;
  isAuthenticated: boolean;
  setSession: (session: Session | null) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const session = useSyncExternalStore(subscribeToSession, getSession);

  const value: AuthContextValue = {
    session,
    isAuthenticated: session !== null,
    setSession,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
