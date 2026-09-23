import { httpClient } from "../../../core/http/httpClient";
import type { CurrentUserResponse, LoginRequest, RegisterRequest, TokenResponse } from "../types";

export async function login(request: LoginRequest): Promise<TokenResponse> {
  const { data } = await httpClient.post<TokenResponse>("/api/auth/login", request);
  return data;
}

export async function register(request: RegisterRequest): Promise<TokenResponse> {
  const { data } = await httpClient.post<TokenResponse>("/api/auth/register", request);
  return data;
}

export async function logout(refreshToken: string): Promise<void> {
  await httpClient.post("/api/auth/logout", { refreshToken });
}

export async function getCurrentUser(): Promise<CurrentUserResponse> {
  const { data } = await httpClient.get<CurrentUserResponse>("/api/auth/me");
  return data;
}
