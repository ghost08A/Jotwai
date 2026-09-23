import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    // Fail loudly if 5173 is taken instead of silently picking another port —
    // a silent port bump breaks the backend's CORS allow-list (Cors:AllowedOrigins)
    // in a way that looks like an unrelated network error.
    strictPort: true,
  },
});
