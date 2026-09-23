import { useMutation } from "@tanstack/react-query";
import { useAuth } from "../../../shared/context/AuthContext";
import { logout } from "../services/authService";

export function useLogout() {
  const { session, setSession } = useAuth();

  return useMutation({
    mutationFn: () => (session ? logout(session.refreshToken) : Promise.resolve()),
    onSettled: () => setSession(null),
  });
}
