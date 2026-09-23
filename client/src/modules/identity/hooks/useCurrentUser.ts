import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../../shared/context/AuthContext";
import { getCurrentUser } from "../services/authService";

export function useCurrentUser() {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: getCurrentUser,
    enabled: isAuthenticated,
  });
}
