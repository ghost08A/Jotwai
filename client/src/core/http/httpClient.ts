import axios from "axios";
import { getSession, setSession } from "./session";

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

httpClient.interceptors.request.use((config) => {
  const session = getSession();
  if (session) {
    config.headers.set("Authorization", `Bearer ${session.accessToken}`);
  }
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const session = getSession();

    if (error.response?.status !== 401 || !session || original?._retried) {
      throw error;
    }
    original._retried = true;

    refreshPromise ??= refreshSession(session.refreshToken);
    const accessToken = await refreshPromise;
    refreshPromise = null;

    if (!accessToken) {
      throw error;
    }

    original.headers.set("Authorization", `Bearer ${accessToken}`);
    return httpClient(original);
  },
);

// Calls the API directly with a bare axios instance rather than through
// authService/httpClient: httpClient's own interceptor would recurse on this
// request, and importing modules/identity/services here would create a
// core -> modules import cycle (services already depend on httpClient).
async function refreshSession(refreshToken: string): Promise<string | null> {
  try {
    const { data } = await axios.post<{ accessToken: string; refreshToken: string }>(
      `${import.meta.env.VITE_API_BASE_URL}/api/auth/refresh`,
      { refreshToken },
    );
    setSession({ accessToken: data.accessToken, refreshToken: data.refreshToken });
    return data.accessToken;
  } catch {
    setSession(null);
    return null;
  }
}
