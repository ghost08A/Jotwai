import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../shared/context/AuthContext";
import { ROUTES } from "../../shared/routes/paths";

export function PublicOnlyRoute() {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) {
    return <Navigate to={ROUTES.home} replace />;
  }

  return <Outlet />;
}
