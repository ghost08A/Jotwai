import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ROUTES } from "../../shared/routes/paths";
import { Home } from "../pages/Home";
import { LoginPage } from "../../modules/identity/components/LoginPage";
import { RegisterPage } from "../../modules/identity/components/RegisterPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { PublicOnlyRoute } from "./PublicOnlyRoute";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<PublicOnlyRoute />}>
          <Route path={ROUTES.login} element={<LoginPage />} />
          <Route path={ROUTES.register} element={<RegisterPage />} />
        </Route>
        <Route element={<ProtectedRoute />}>
          <Route path={ROUTES.home} element={<Home />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
