import { Outlet } from "react-router-dom";
import { useAuth } from "../../shared/context/AuthContext";
import { Unauthorized } from "../pages/Unauthorized";

export function ProtectedRoute() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Unauthorized />;
  }

  return <Outlet />;
}
