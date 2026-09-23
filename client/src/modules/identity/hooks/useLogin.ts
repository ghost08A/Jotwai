import { useMutation } from "@tanstack/react-query";
import { useAuth } from "../../../shared/context/AuthContext";
import { login } from "../services/authService";
import type { LoginRequest } from "../types";

export function useLogin() {
  const { setSession } = useAuth();

  return useMutation({
    mutationFn: (request: LoginRequest) => login(request),
    onSuccess: (tokens) => {
      setSession({ accessToken: tokens.accessToken, refreshToken: tokens.refreshToken });
    },
  });
}
