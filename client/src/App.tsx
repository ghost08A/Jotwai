import { AppProviders } from "./core/providers/AppProviders";
import { AppRouter } from "./core/router/AppRouter";

export function App() {
  return (
    <AppProviders>
      <AppRouter />
    </AppProviders>
  );
}
