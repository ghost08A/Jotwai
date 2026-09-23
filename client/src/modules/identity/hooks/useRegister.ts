import { useMutation } from "@tanstack/react-query";
import { useAuth } from "../../../shared/context/AuthContext";
import { register } from "../services/authService";
import type { RegisterRequest } from "../types";

export function useRegister() {
  const { setSession } = useAuth();

  return useMutation({
    mutationFn: (request: RegisterRequest) => register(request),
    onSuccess: (tokens) => {
      setSession({ accessToken: tokens.accessToken, refreshToken: tokens.refreshToken });
    },
  });
}
